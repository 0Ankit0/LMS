from __future__ import annotations

from datetime import datetime
from typing import Optional

from sqlalchemy import Text
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column

from src.db.base import Base


class GeneralSetting(Base):
    __tablename__ = "generalsetting"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    key: Mapped[str] = mapped_column(unique=True, index=True)
    env_value: Mapped[Optional[str]] = mapped_column(Text, default=None)
    db_value: Mapped[Optional[str]] = mapped_column(Text, default=None)
    use_db_value: Mapped[bool] = mapped_column(default=False)
    is_runtime_editable: Mapped[bool] = mapped_column(default=True)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    updated_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
