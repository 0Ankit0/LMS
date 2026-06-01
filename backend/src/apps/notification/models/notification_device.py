from __future__ import annotations

from datetime import datetime
from enum import Enum
from typing import TYPE_CHECKING, Any, Optional

from sqlalchemy import Enum as SAEnum, ForeignKey, JSON, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from src.db.base import Base

if TYPE_CHECKING:
    from src.apps.iam.models.user import User


class NotificationDeviceProvider(str, Enum):
    WEBPUSH = "webpush"
    FCM = "fcm"
    ONESIGNAL = "onesignal"


class NotificationDevicePlatform(str, Enum):
    WEB = "web"
    ANDROID = "android"
    IOS = "ios"


class NotificationDevice(Base):
    __tablename__ = "notificationdevice"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    provider: Mapped[NotificationDeviceProvider] = mapped_column(
        SAEnum(
            NotificationDeviceProvider,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        index=True,
    )
    platform: Mapped[NotificationDevicePlatform] = mapped_column(
        SAEnum(
            NotificationDevicePlatform,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        index=True,
    )
    token: Mapped[Optional[str]] = mapped_column(String(2048), default=None)
    endpoint: Mapped[Optional[str]] = mapped_column(String(2048), default=None)
    p256dh: Mapped[Optional[str]] = mapped_column(String(512), default=None)
    auth: Mapped[Optional[str]] = mapped_column(String(256), default=None)
    subscription_id: Mapped[Optional[str]] = mapped_column(String(255), default=None, index=True)
    device_metadata: Mapped[Optional[Any]] = mapped_column(JSON, default=None)
    is_active: Mapped[bool] = mapped_column(default=True, index=True)
    last_seen_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    updated_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    user: Mapped["User | None"] = relationship(back_populates="notification_devices", init=False)
