from __future__ import annotations

from typing import Optional, TYPE_CHECKING
from datetime import datetime
from sqlalchemy import ForeignKey, String
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column, relationship

from src.db.base import Base

if TYPE_CHECKING:
    from .login_attempt import LoginAttempt
    from .token_tracking import TokenTracking
    from .used_token import UsedToken
    from .role import UserRole
    from src.apps.multitenancy.models.tenant import Tenant, TenantMember, TenantInvitation
    from src.apps.notification.models.notification_device import NotificationDevice
    from src.apps.notification.models.notification import Notification
    from src.apps.notification.models.notification_preference import NotificationPreference

class UserBase(MappedAsDataclass, kw_only=True):
    username: Mapped[str] = mapped_column(String(50), unique=True, index=True)
    email: Mapped[str] = mapped_column(String(255), unique=True, index=True)
    is_active: Mapped[bool] = mapped_column(default=True)
    is_superuser: Mapped[bool] = mapped_column(default=False)
    is_confirmed: Mapped[bool] = mapped_column(default=False)
    otp_enabled: Mapped[bool] = mapped_column(default=False)
    otp_verified: Mapped[bool] = mapped_column(default=False)
    otp_base32: Mapped[str] = mapped_column(String(255), default="")
    otp_auth_url: Mapped[str] = mapped_column(String(255), default="")


class User(UserBase, Base):
    __tablename__ = "user"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    hashed_password: Mapped[Optional[str]] = mapped_column(String(255), default=None)
    social_provider: Mapped[Optional[str]] = mapped_column(String(50), default=None, index=True)
    social_id: Mapped[Optional[str]] = mapped_column(String(255), default=None, index=True)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    profile: Mapped[UserProfile | None] = relationship(
        back_populates="user",
        init=False,
        lazy="selectin",
    )
    login_attempts: Mapped[list["LoginAttempt"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    tokens: Mapped[list["TokenTracking"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    used_tokens: Mapped[list["UsedToken"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    user_roles: Mapped[list["UserRole"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    owned_tenants: Mapped[list["Tenant"]] = relationship(
        back_populates="owner",
        init=False,
        default_factory=list,
        foreign_keys="Tenant.owner_id",
    )
    tenant_memberships: Mapped[list["TenantMember"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    sent_invitations: Mapped[list["TenantInvitation"]] = relationship(
        back_populates="inviter",
        init=False,
        default_factory=list,
        foreign_keys="TenantInvitation.invited_by",
    )
    notifications: Mapped[list["Notification"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    notification_devices: Mapped[list["NotificationDevice"]] = relationship(
        back_populates="user",
        init=False,
        default_factory=list,
    )
    notification_preference: Mapped["NotificationPreference | None"] = relationship(
        back_populates="user",
        init=False,
    )


class UserProfileBase(MappedAsDataclass, kw_only=True):
    first_name: Mapped[str] = mapped_column(String(40), default="")
    last_name: Mapped[str] = mapped_column(String(40), default="")
    phone: Mapped[str] = mapped_column(String(20), default="")
    image_url: Mapped[str] = mapped_column(String(255), default="")
    bio: Mapped[str] = mapped_column(String(500), default="")


class UserProfile(UserProfileBase, Base):
    __tablename__ = "userprofile"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)

    user: Mapped[User | None] = relationship(back_populates="profile", init=False, lazy="selectin")
