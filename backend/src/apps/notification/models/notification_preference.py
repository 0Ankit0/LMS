"""User notification channel preference model."""
from __future__ import annotations

from typing import TYPE_CHECKING, Optional

from sqlalchemy import ForeignKey, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from src.db.base import Base

if TYPE_CHECKING:
    from src.apps.iam.models.user import User


class NotificationPreference(Base):
    """
    Stores per-user notification channel preferences.

    A row is auto-created (with all defaults) the first time a user's
    preferences are read or updated.  The *_enabled flags gate whether a
    given channel is used when a notification is dispatched.
    """
    __tablename__ = "notificationpreference"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), unique=True, index=True)

    # ── Channel flags ──────────────────────────────────────────────────────
    websocket_enabled: Mapped[bool] = mapped_column(default=True)
    email_enabled: Mapped[bool] = mapped_column(default=False)
    push_enabled: Mapped[bool] = mapped_column(default=False)
    sms_enabled: Mapped[bool] = mapped_column(default=False)

    # ── Web-Push subscription data (populated by the browser after opt-in) ─
    push_endpoint: Mapped[Optional[str]] = mapped_column(String(2048), default=None)
    push_p256dh: Mapped[Optional[str]] = mapped_column(String(512), default=None)
    push_auth: Mapped[Optional[str]] = mapped_column(String(256), default=None)

    user: Mapped["User | None"] = relationship(
        back_populates="notification_preference",
        init=False,
    )
