"""Notification ORM model."""
from __future__ import annotations

from datetime import datetime
from enum import Enum
from typing import TYPE_CHECKING, Any, Optional

from sqlalchemy import Enum as SAEnum, ForeignKey, JSON, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from src.db.base import Base

if TYPE_CHECKING:
    from src.apps.iam.models.user import User


class NotificationType(str, Enum):
    INFO = "info"
    SUCCESS = "success"
    WARNING = "warning"
    ERROR = "error"
    PAYMENT = "payment"
    AUTH = "auth"
    SYSTEM = "system"


class Notification(Base):
    __tablename__ = "notification"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    title: Mapped[str] = mapped_column(String(255))
    body: Mapped[str] = mapped_column(String(2000))
    type: Mapped[NotificationType] = mapped_column(
        SAEnum(
            NotificationType,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        default=NotificationType.INFO,
    )
    is_read: Mapped[bool] = mapped_column(default=False)
    extra_data: Mapped[Optional[Any]] = mapped_column(JSON, default=None)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    user: Mapped["User | None"] = relationship(back_populates="notifications", init=False)
