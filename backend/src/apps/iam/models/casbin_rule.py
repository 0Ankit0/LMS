from __future__ import annotations

from typing import Optional

from sqlalchemy import String
from sqlalchemy.orm import Mapped, mapped_column

from src.db.base import Base


class CasbinRule(Base):
    """
    Casbin policy rule storage model.

    The adapter persists both permission policies (`ptype="p"`) and role/grouping
    tuples (`ptype="g"`) into the same table. For this project's model:

    - `p` rows map to `(role, domain, resource, action)`
    - `g` rows map to `(user_id, role, domain)`

    Table name must match what `casbin-async-sqlalchemy-adapter` expects.
    """
    __tablename__ = "casbin_rule"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    ptype: Mapped[str] = mapped_column(String(255), index=True)
    v0: Mapped[Optional[str]] = mapped_column(String(255), default="", nullable=True, index=True)
    v1: Mapped[Optional[str]] = mapped_column(String(255), default="", nullable=True, index=True)
    v2: Mapped[Optional[str]] = mapped_column(String(255), default="", nullable=True, index=True)
    v3: Mapped[Optional[str]] = mapped_column(String(255), default="", nullable=True)
    v4: Mapped[Optional[str]] = mapped_column(String(255), default="", nullable=True)
    v5: Mapped[Optional[str]] = mapped_column(String(255), default="", nullable=True)
