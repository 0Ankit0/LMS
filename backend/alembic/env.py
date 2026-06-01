from logging.config import fileConfig
from sqlalchemy import create_engine
from sqlalchemy import pool
from alembic import context
import sys
from pathlib import Path

# Add project root to path so modules can be imported
sys.path.append(str(Path(__file__).parent.parent))

# Import your settings and models
from src.apps.core.config import settings
from src.db.base import Base
from src.apps.core.models import *  # noqa: F403,F401
from src.apps.finance.models import *  # noqa: F403,F401
from src.apps.iam.models import *  # noqa: F403,F401
from src.apps.multitenancy.models import *  # noqa: F403,F401
from src.apps.notification.models import *  # noqa: F403,F401
from src.apps.observability.models import *  # noqa: F403,F401

# this is the Alembic Config object, which provides
# access to the values within the .ini file in use.
config = context.config

# Interpret the config file for Python logging.
if config.config_file_name is not None:
    fileConfig(config.config_file_name)

# Set target metadata from SQLAlchemy declarative models
target_metadata = Base.metadata

def run_migrations_offline() -> None:
    url = settings.DATABASE_URL
    context.configure(
        url=url,
        target_metadata=target_metadata,
        literal_binds=True,
        dialect_opts={"paramstyle": "named"},
    )
    with context.begin_transaction():
        context.run_migrations()

def run_migrations_online() -> None:
    connectable = create_engine(
        settings.DATABASE_URL,
        poolclass=pool.NullPool,
    )
    with connectable.connect() as connection:
        context.configure(
            connection=connection,
            target_metadata=target_metadata,
        )
        with context.begin_transaction():
            context.run_migrations()
if context.is_offline_mode():
    run_migrations_offline()
else:
    run_migrations_online()
