from __future__ import annotations

from typing import Optional, TYPE_CHECKING
from datetime import datetime
from sqlalchemy import ForeignKey, String
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column, relationship

from src.db.base import Base

if TYPE_CHECKING:
    from .user import User


class UsedTokenBase(MappedAsDataclass, kw_only=True):
    token_jti: Mapped[str] = mapped_column(String(255), index=True, unique=True)
    token_purpose: Mapped[str] = mapped_column(String(50))
    used_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)


class UsedToken(UsedTokenBase, Base):
    __tablename__ = "usedtoken"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    user_id: Mapped[Optional[int]] = mapped_column(
        ForeignKey("user.id", ondelete="CASCADE"),
        default=None,
    )

    user: Mapped[User | None] = relationship(back_populates="used_tokens", init=False)
