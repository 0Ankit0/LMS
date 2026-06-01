from __future__ import annotations

from datetime import datetime, timezone
from typing import Any

from sqlalchemy import ForeignKey, JSON, String
from sqlalchemy.orm import Mapped, mapped_column

from src.db.base import Base


def utc_now() -> datetime:
    return datetime.now(timezone.utc).replace(tzinfo=None)


class ObservabilityLogEntry(Base):
    __tablename__ = "observabilitylogentry"

    id: Mapped[int | None] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    level: Mapped[str] = mapped_column(String(16), index=True)
    logger_name: Mapped[str] = mapped_column(String(255), index=True)
    source: Mapped[str] = mapped_column(String(64), index=True)
    message: Mapped[str] = mapped_column(String(1024))
    timestamp: Mapped[datetime] = mapped_column(default_factory=utc_now, index=True)
    event_code: Mapped[str] = mapped_column(String(128), default="", index=True)
    request_id: Mapped[str] = mapped_column(String(64), default="", index=True)
    method: Mapped[str] = mapped_column(String(16), default="", index=True)
    path: Mapped[str] = mapped_column(String(255), default="", index=True)
    status_code: Mapped[int | None] = mapped_column(default=None, index=True)
    duration_ms: Mapped[int | None] = mapped_column(default=None)
    user_id: Mapped[int | None] = mapped_column(ForeignKey("user.id"), default=None, index=True)
    ip_address: Mapped[str] = mapped_column(String(45), default="", index=True)
    user_agent: Mapped[str] = mapped_column(String(255), default="")
    metadata_json: Mapped[dict[str, Any]] = mapped_column("metadata", JSON, default_factory=dict)


class SecurityIncident(Base):
    __tablename__ = "securityincident"

    id: Mapped[int | None] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    signal_type: Mapped[str] = mapped_column(String(128), index=True)
    severity: Mapped[str] = mapped_column(String(16), index=True)
    title: Mapped[str] = mapped_column(String(255))
    summary: Mapped[str] = mapped_column(String(1024))
    fingerprint: Mapped[str] = mapped_column(String(255), index=True)
    status: Mapped[str] = mapped_column(String(16), default="open", index=True)
    occurrence_count: Mapped[int] = mapped_column(default=1)
    first_seen_at: Mapped[datetime] = mapped_column(default_factory=utc_now, index=True)
    last_seen_at: Mapped[datetime] = mapped_column(default_factory=utc_now, index=True)
    actor_user_id: Mapped[int | None] = mapped_column(ForeignKey("user.id"), default=None, index=True)
    subject_user_id: Mapped[int | None] = mapped_column(ForeignKey("user.id"), default=None, index=True)
    ip_address: Mapped[str] = mapped_column(String(45), default="", index=True)
    related_log_id: Mapped[int | None] = mapped_column(
        ForeignKey("observabilitylogentry.id"),
        default=None,
        index=True,
    )
    metadata_json: Mapped[dict[str, Any]] = mapped_column("metadata", JSON, default_factory=dict)
    review_notes: Mapped[str] = mapped_column(String(2048), default="")
    reviewed_by: Mapped[int | None] = mapped_column(ForeignKey("user.id"), default=None, index=True)
    reviewed_at: Mapped[datetime | None] = mapped_column(default=None)
