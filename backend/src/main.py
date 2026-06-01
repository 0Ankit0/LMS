from contextlib import asynccontextmanager
import os
from fastapi import FastAPI
from fastapi.responses import RedirectResponse
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.trustedhost import TrustedHostMiddleware
from fastapi.staticfiles import StaticFiles
from slowapi import Limiter
from slowapi.util import get_remote_address
from slowapi.errors import RateLimitExceeded
from uvicorn.middleware.proxy_headers import ProxyHeadersMiddleware
from src.apps.core.config import settings
from src.apps.core.handler import rate_limit_exceeded_handler
from src.apps.core.logging import configure_logging
from src.apps.core.middleware import RequestContextMiddleware, SecurityHeadersMiddleware
from src.apps.iam.api import api_router
from src.db.session import engine, init_db
from src.apps.iam.casbin_enforcer import CasbinEnforcer
from src.apps.system.api import router as system_router
from src.apps.observability.api import router as observability_router
from src.apps.observability.service import prune_old_log_entries
from src.apps.core.storage import storage_uses_local_filesystem
from src.apps.lms import router as lms_router

configure_logging()

# Rate limiter
limiter = Limiter(
    key_func=get_remote_address,
    default_limits=[lambda: settings.RATE_LIMIT_DEFAULT],
)

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Initialize DB tables, Casbin enforcer, Redis cache, WebSocket manager, and Analytics on startup."""
    await init_db()

    enforcer = await CasbinEnforcer.get_enforcer(engine)
    app.state.casbin_enforcer = enforcer

    app.state.ws_manager = None
    app.state.analytics = None

    # Initialize Redis cache + WebSocket pub/sub in production when enabled
    if settings.FEATURE_WEBSOCKETS:
        from src.apps.websocket.manager import manager as ws_manager

        app.state.ws_manager = ws_manager
        if not settings.DEBUG and settings.REDIS_URL:
            from src.apps.core.cache import RedisCache

            await RedisCache.get_client()
            await ws_manager.setup_redis(settings.REDIS_URL)

    # Analytics service (no-op when disabled)
    if settings.FEATURE_ANALYTICS:
        from src.apps.analytics import init_analytics

        app.state.analytics = init_analytics()

    from src.db.session import async_session_factory
    async with async_session_factory() as session:
        await prune_old_log_entries(session)
        await session.commit()

    yield

    # Cleanup on shutdown
    if settings.FEATURE_WEBSOCKETS and app.state.ws_manager is not None:
        await app.state.ws_manager.teardown()
        if settings.REDIS_URL:
            from src.apps.core.cache import RedisCache

            await RedisCache.close()

    if settings.FEATURE_ANALYTICS:
        from src.apps.analytics import shutdown_analytics

        await shutdown_analytics()

app = FastAPI(
    lifespan=lifespan,
    title=settings.PROJECT_NAME,
    description=f"{settings.PROJECT_NAME} API",
    version="0.1.0",
    swagger_ui_parameters={
        "syntaxHighlight.theme": "monokai",  # Syntax highlighting theme
        "deepLinking": True,  # Enable deep linking to operations
        "displayOperationId": True,  # Show operation IDs
        "filter": True,  # Enable search/filter bar
        "showExtensions": True,  # Show vendor extensions
        "showCommonExtensions": True,
        "persistAuthorization": True,  # Remember authorization between reloads
        "displayRequestDuration": True,  # Show request duration
        "docExpansion": "list",  # Default expansion: "list", "full", or "none"
        "defaultModelsExpandDepth": 1,  # How deep to expand models
        "defaultModelExpandDepth": 1,
    }
)

# Add rate limiter to app state and register exception handler
app.state.limiter = limiter
app.add_exception_handler(RateLimitExceeded, rate_limit_exceeded_handler)

# Trust proxy headers (X-Forwarded-For / X-Real-IP) so request.client.host
# reflects the real client IP rather than the loopback / proxy address.
app.add_middleware(
    ProxyHeadersMiddleware,
    trusted_hosts=settings.PROXY_TRUSTED_HOSTS or settings.FORWARDED_ALLOW_IPS,
)

# Security headers middleware
app.add_middleware(SecurityHeadersMiddleware)

# Request context and persisted request logging
app.add_middleware(RequestContextMiddleware)

# Analytics request-tracking middleware
if settings.FEATURE_ANALYTICS:
    from src.apps.analytics.middleware import AnalyticsMiddleware

    app.add_middleware(AnalyticsMiddleware)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"] if settings.DEBUG else [str(origin) for origin in settings.BACKEND_CORS_ORIGINS],
    allow_credentials=not settings.DEBUG,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Trusted host middleware (prevent host header attacks)
if not settings.DEBUG and not settings.TESTING:
    app.add_middleware(
        TrustedHostMiddleware,
        allowed_hosts=settings.TRUSTED_HOSTS,
    )

app.include_router(system_router, prefix=settings.API_V1_STR)
app.include_router(observability_router, prefix=settings.API_V1_STR)

if settings.FEATURE_AUTH:
    app.include_router(api_router, prefix=settings.API_V1_STR)
if settings.FEATURE_FINANCE:
    from src.apps.finance.api import finance_router

    app.include_router(finance_router, prefix=settings.API_V1_STR)
if settings.FEATURE_MULTITENANCY:
    from src.apps.multitenancy.api import multitenancy_router

    app.include_router(multitenancy_router, prefix=settings.API_V1_STR)
if settings.FEATURE_WEBSOCKETS:
    from src.apps.websocket.api import ws_router

    app.include_router(ws_router, prefix=settings.API_V1_STR)
if settings.FEATURE_NOTIFICATIONS:
    from src.apps.notification.api import notification_router

    app.include_router(notification_router, prefix=settings.API_V1_STR)
if settings.FEATURE_ANALYTICS:
    from src.apps.analytics.api import router as analytics_router

    app.include_router(analytics_router, prefix=settings.API_V1_STR)

# LMS endpoints are exposed using documented paths (e.g. /api/courses, /register)
app.include_router(lms_router)

# Serve uploaded media files (avatars, etc.)
if storage_uses_local_filesystem():
    os.makedirs(settings.MEDIA_DIR, exist_ok=True)
    app.mount(settings.MEDIA_URL, StaticFiles(directory=settings.MEDIA_DIR), name="media")

@app.get("/", include_in_schema=False)
async def read_root() -> RedirectResponse:
    """Redirect root to the interactive API documentation."""
    return RedirectResponse(url="/docs")
