from __future__ import annotations

from typing import Optional, TYPE_CHECKING
from datetime import datetime
from sqlalchemy import Enum as SAEnum, ForeignKey, String
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column, relationship
from src.apps.core.security import TokenType
from src.db.base import Base

if TYPE_CHECKING:
    from .user import User


class TokenTrackingBase(MappedAsDataclass, kw_only=True):
    token_jti: Mapped[str] = mapped_column(String(255), index=True, unique=True)
    token_type: Mapped[TokenType] = mapped_column(
        SAEnum(
            TokenType,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            name="tokentype",
        )
    )
    ip_address: Mapped[str] = mapped_column(String(45))
    user_agent: Mapped[str] = mapped_column(String(255))
    expires_at: Mapped[datetime]
    is_active: Mapped[bool] = mapped_column(default=True)
    revoked_at: Mapped[Optional[datetime]] = mapped_column(default=None)
    revoke_reason: Mapped[str] = mapped_column(String(255), default="")
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)


class TokenTracking(TokenTrackingBase, Base):
    __tablename__ = "tokentracking"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[Optional[int]] = mapped_column(
        ForeignKey("user.id", ondelete="CASCADE"),
        default=None,
    )

    user: Mapped[User | None] = relationship(back_populates="tokens", init=False)
