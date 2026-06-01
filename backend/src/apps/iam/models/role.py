from __future__ import annotations

from typing import Optional
from datetime import datetime
from sqlalchemy import ForeignKey, String
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column, relationship

from src.db.base import Base


class RoleBase(MappedAsDataclass, kw_only=True):
    name: Mapped[str] = mapped_column(String(50), unique=True, index=True)
    description: Mapped[str] = mapped_column(String(255), default="")


class Role(RoleBase, Base):
    __tablename__ = "role"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    updated_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    user_roles: Mapped[list["UserRole"]] = relationship(
        back_populates="role",
        init=False,
        default_factory=list,
    )
    role_permissions: Mapped[list["RolePermission"]] = relationship(
        back_populates="role",
        init=False,
        default_factory=list,
    )


class PermissionBase(MappedAsDataclass, kw_only=True):
    resource: Mapped[str] = mapped_column(String(100), index=True)
    action: Mapped[str] = mapped_column(String(50), index=True)
    description: Mapped[str] = mapped_column(String(255), default="")


class Permission(PermissionBase, Base):
    __tablename__ = "permission"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    role_permissions: Mapped[list["RolePermission"]] = relationship(
        back_populates="permission",
        init=False,
        default_factory=list,
    )


class UserRole(Base):
    __tablename__ = "userrole"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    role_id: Mapped[int] = mapped_column(ForeignKey("role.id"), index=True)
    assigned_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    user: Mapped["User | None"] = relationship(back_populates="user_roles", init=False)
    role: Mapped[Role | None] = relationship(back_populates="user_roles", init=False)


class RolePermission(Base):
    __tablename__ = "rolepermission"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    role_id: Mapped[int] = mapped_column(ForeignKey("role.id"), index=True)
    permission_id: Mapped[int] = mapped_column(ForeignKey("permission.id"), index=True)
    granted_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)

    role: Mapped[Role | None] = relationship(back_populates="role_permissions", init=False)
    permission: Mapped[Permission | None] = relationship(
        back_populates="role_permissions",
        init=False,
    )
