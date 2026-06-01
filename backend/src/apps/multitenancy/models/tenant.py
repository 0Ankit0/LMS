from __future__ import annotations

from typing import Optional, TYPE_CHECKING
from datetime import datetime
from enum import Enum
from sqlalchemy import Enum as SAEnum, ForeignKey, String
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column, relationship

from src.db.base import Base

if TYPE_CHECKING:
    from src.apps.iam.models.user import User


class TenantRole(str, Enum):
    OWNER = "owner"
    ADMIN = "admin"
    MEMBER = "member"


class InvitationStatus(str, Enum):
    PENDING = "pending"
    ACCEPTED = "accepted"
    EXPIRED = "expired"
    REVOKED = "revoked"


# ── Tenant ───────────────────────────────────────────────────────────────────

class TenantBase(MappedAsDataclass, kw_only=True):
    name: Mapped[str] = mapped_column(String(100))
    slug: Mapped[str] = mapped_column(String(63), unique=True, index=True)
    description: Mapped[str] = mapped_column(String(500), default="")
    is_active: Mapped[bool] = mapped_column(default=True)


class Tenant(TenantBase, Base):
    __tablename__ = "tenant"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    owner_id: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    updated_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    members: Mapped[list["TenantMember"]] = relationship(
        back_populates="tenant",
        init=False,
        default_factory=list,
    )
    invitations: Mapped[list["TenantInvitation"]] = relationship(
        back_populates="tenant",
        init=False,
        default_factory=list,
    )
    owner: Mapped["User | None"] = relationship(
        back_populates="owned_tenants",
        init=False,
        foreign_keys="Tenant.owner_id",
    )


# ── TenantMember ─────────────────────────────────────────────────────────────

class TenantMemberBase(MappedAsDataclass, kw_only=True):
    role: Mapped[TenantRole] = mapped_column(
        SAEnum(
            TenantRole,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        default=TenantRole.MEMBER,
    )
    is_active: Mapped[bool] = mapped_column(default=True)


class TenantMember(TenantMemberBase, Base):
    __tablename__ = "tenantmember"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    tenant_id: Mapped[Optional[int]] = mapped_column(
        ForeignKey("tenant.id", ondelete="CASCADE"),
        default=None,
        index=True,
    )
    user_id: Mapped[Optional[int]] = mapped_column(
        ForeignKey("user.id", ondelete="CASCADE"),
        default=None,
        index=True,
    )
    joined_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    tenant: Mapped[Tenant | None] = relationship(back_populates="members", init=False)
    user: Mapped["User | None"] = relationship(back_populates="tenant_memberships", init=False)


# ── TenantInvitation ─────────────────────────────────────────────────────────

class TenantInvitationBase(MappedAsDataclass, kw_only=True):
    email: Mapped[str] = mapped_column(String(255), index=True)
    role: Mapped[TenantRole] = mapped_column(
        SAEnum(
            TenantRole,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        default=TenantRole.MEMBER,
    )
    status: Mapped[InvitationStatus] = mapped_column(
        SAEnum(
            InvitationStatus,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        default=InvitationStatus.PENDING,
    )


class TenantInvitation(TenantInvitationBase, Base):
    __tablename__ = "tenantinvitation"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    tenant_id: Mapped[Optional[int]] = mapped_column(
        ForeignKey("tenant.id", ondelete="CASCADE"),
        default=None,
        index=True,
    )
    invited_by: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)
    token: Mapped[str] = mapped_column(String(255), unique=True, index=True)
    expires_at: Mapped[datetime]
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    accepted_at: Mapped[Optional[datetime]] = mapped_column(default=None)

    tenant: Mapped[Tenant | None] = relationship(back_populates="invitations", init=False)
    inviter: Mapped["User | None"] = relationship(
        back_populates="sent_invitations",
        init=False,
        foreign_keys="TenantInvitation.invited_by",
    )
