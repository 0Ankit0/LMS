import pytest
import os
import re
from contextlib import ExitStack
from typing import AsyncGenerator
from unittest.mock import AsyncMock, patch
from httpx import ASGITransport, AsyncClient
from sqlalchemy import create_engine, text
from sqlalchemy.engine import make_url
from sqlalchemy.ext.asyncio import AsyncSession, create_async_engine, async_sessionmaker
from sqlalchemy.pool import NullPool
from src.db.base import Base

os.environ["TESTING"] = "True"

_test_postgres_server = os.environ.get("TEST_POSTGRES_SERVER") or os.environ.get("POSTGRES_SERVER", "localhost")
_test_postgres_user = os.environ.get("TEST_POSTGRES_USER") or os.environ.get("POSTGRES_USER", "postgres")
_test_postgres_password = os.environ.get("TEST_POSTGRES_PASSWORD") or os.environ.get("POSTGRES_PASSWORD", "postgres")
_base_postgres_db = os.environ.get("POSTGRES_DB", "app")
_test_postgres_db = os.environ.get("TEST_POSTGRES_DB") or f"{_base_postgres_db}_test"

os.environ.setdefault("POSTGRES_SERVER", _test_postgres_server)
os.environ.setdefault("POSTGRES_USER", _test_postgres_user)
os.environ.setdefault("POSTGRES_PASSWORD", _test_postgres_password)
os.environ.setdefault("POSTGRES_DB", _test_postgres_db)

TEST_DATABASE_URL = os.environ.setdefault(
    "DATABASE_URL",
    os.environ.get("TEST_DATABASE_URL")
    or (
        f"postgresql+psycopg://{_test_postgres_user}:{_test_postgres_password}"
        f"@{_test_postgres_server}/{_test_postgres_db}"
    ),
)

from src.main import app
from src.db import session as db_session_module
from src.db.session import get_session


def _load_models() -> None:
    import src.apps.core.models  # noqa: F401
    import src.apps.finance.models  # noqa: F401
    import src.apps.iam.models  # noqa: F401
    import src.apps.multitenancy.models  # noqa: F401
    import src.apps.notification.models  # noqa: F401
    import src.apps.observability.models  # noqa: F401
    import src.apps.websocket.models  # noqa: F401
    import src.apps.lms.models  # noqa: F401


def _validated_database_name(database_url: str) -> str:
    database_name = make_url(database_url).database
    if not database_name or not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_]*", database_name):
        raise RuntimeError(
            "TEST_DATABASE_URL must point to a PostgreSQL database with a simple identifier name"
        )
    return database_name


def _admin_database_url(database_url: str) -> str:
    return make_url(database_url).set(database="postgres").render_as_string(hide_password=False)


def _ensure_test_database() -> None:
    database_name = _validated_database_name(TEST_DATABASE_URL)
    admin_engine = create_engine(
        _admin_database_url(TEST_DATABASE_URL),
        isolation_level="AUTOCOMMIT",
    )
    try:
        with admin_engine.connect() as connection:
            exists = connection.execute(
                text("SELECT 1 FROM pg_database WHERE datname = :database_name"),
                {"database_name": database_name},
            ).scalar()
            if not exists:
                connection.execute(text(f'CREATE DATABASE "{database_name}"'))
    finally:
        admin_engine.dispose()


@pytest.fixture(scope="function")
async def test_engine():
    """Create a PostgreSQL-backed test engine for each test function."""
    _ensure_test_database()
    _load_models()
    engine = create_async_engine(
        TEST_DATABASE_URL,
        echo=False,
        poolclass=NullPool,
    )

    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.drop_all)
        await conn.run_sync(Base.metadata.create_all)

    yield engine

    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.drop_all)

    await engine.dispose()


@pytest.fixture(scope="function")
async def db_session(test_engine) -> AsyncGenerator[AsyncSession, None]:
    """Create a fresh database session for each test."""
    async_session = async_sessionmaker(
        test_engine,
        class_=AsyncSession,
        expire_on_commit=False
    )
    
    async with async_session() as session:
        yield session


@pytest.fixture(scope="function")
async def client(db_session: AsyncSession) -> AsyncGenerator[AsyncClient, None]:
    """Create a test client with database session override and disabled rate limiting."""
    from src.apps.iam.api.deps import get_db
    from src.apps.analytics.service import AnalyticsService
    from src.apps.analytics.dependencies import get_analytics

    async def override_get_db() -> AsyncGenerator[AsyncSession, None]:
        yield db_session

    # Provide a disabled (no-op) analytics service for tests
    _noop_analytics = AnalyticsService(provider=None)
    app.dependency_overrides[get_db] = override_get_db
    app.dependency_overrides[get_session] = override_get_db
    app.dependency_overrides[get_analytics] = lambda: _noop_analytics

    original_async_session_factory = db_session_module.async_session_factory
    test_async_session = async_sessionmaker(
        db_session.bind,
        class_=AsyncSession,
        expire_on_commit=False,
    )
    db_session_module.async_session_factory = test_async_session
    
    # Disable rate limiting for tests - handle both main limiter and route limiters
    if hasattr(app.state, 'limiter'):
        original_enabled = app.state.limiter.enabled
        app.state.limiter.enabled = False
    else:
        original_enabled = None
    
    # Also disable limiters in individual route modules
    limiters_to_restore = []
    try:
        from src.apps.iam.api.v1.auth import signup, login, password
        for module in [signup, login, password]:
            if hasattr(module, 'limiter'):
                limiters_to_restore.append((module.limiter, module.limiter.enabled))
                module.limiter.enabled = False
    except Exception:
        pass
    
    # Mock email service if available to avoid sending real emails.
    with ExitStack() as stack:
        for target in [
            "src.apps.iam.services.email.EmailService.send_welcome_email",
            "src.apps.iam.services.email.EmailService.send_verification_email",
            "src.apps.iam.services.email.EmailService.send_password_reset_email",
        ]:
            try:
                stack.enter_context(patch(target, new_callable=AsyncMock))
            except Exception:
                pass

        async with AsyncClient(
            transport=ASGITransport(app=app),
            base_url="http://test"
        ) as test_client:
            yield test_client
    
    # Restore rate limiting after test
    if original_enabled is not None:
        app.state.limiter.enabled = original_enabled
    
    # Restore module limiters
    for limiter, was_enabled in limiters_to_restore:
        limiter.enabled = was_enabled
    
    db_session_module.async_session_factory = original_async_session_factory
    app.dependency_overrides.clear()
