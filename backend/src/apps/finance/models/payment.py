from __future__ import annotations

from datetime import datetime
from enum import Enum
from typing import Optional
from sqlalchemy import Enum as SAEnum, ForeignKey, String, Text
from sqlalchemy.orm import Mapped, MappedAsDataclass, mapped_column

from src.db.base import Base


class PaymentProvider(str, Enum):
    KHALTI = "khalti"
    ESEWA = "esewa"
    STRIPE = "stripe"
    PAYPAL = "paypal"


class PaymentStatus(str, Enum):
    PENDING = "pending"
    INITIATED = "initiated"
    COMPLETED = "completed"
    FAILED = "failed"
    REFUNDED = "refunded"
    CANCELLED = "cancelled"


class PaymentTransactionBase(MappedAsDataclass, kw_only=True):
    """Fields shared between table model and validation schemas."""
    provider: Mapped[PaymentProvider] = mapped_column(
        SAEnum(
            PaymentProvider,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        )
    )
    amount: Mapped[int]
    purchase_order_id: Mapped[str] = mapped_column(String(255), index=True)
    purchase_order_name: Mapped[str] = mapped_column(String(255))
    return_url: Mapped[str] = mapped_column(String(500))
    currency: Mapped[str] = mapped_column(String(3), default="NPR")
    status: Mapped[PaymentStatus] = mapped_column(
        SAEnum(
            PaymentStatus,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        ),
        default=PaymentStatus.PENDING,
    )
    # Provider-assigned identifiers
    provider_transaction_id: Mapped[Optional[str]] = mapped_column(String(255), default=None, index=True)
    provider_pidx: Mapped[Optional[str]] = mapped_column(String(255), default=None, index=True)
    # Redirect / callback URLs
    website_url: Mapped[str] = mapped_column(String(500), default="")
    # Optional FK to the platform user who initiated the payment
    user_id: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)
    # Extra provider-specific data stored as JSON string
    extra_data: Mapped[Optional[str]] = mapped_column(Text, default=None)
    failure_reason: Mapped[Optional[str]] = mapped_column(String(500), default=None)


class PaymentTransaction(PaymentTransactionBase, Base):
    __tablename__ = "payment_transactions"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    created_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
    updated_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)


class PaymentWebhookBase(MappedAsDataclass, kw_only=True):
    """Raw webhook / callback payload received from a payment provider."""
    provider: Mapped[PaymentProvider] = mapped_column(
        SAEnum(
            PaymentProvider,
            values_callable=lambda enum_cls: [member.value for member in enum_cls],
            native_enum=False,
        )
    )
    raw_payload: Mapped[str] = mapped_column(Text)
    event_type: Mapped[str] = mapped_column(String(100), default="callback")
    transaction_id: Mapped[Optional[int]] = mapped_column(
        ForeignKey("payment_transactions.id"),
        default=None,
    )
    is_verified: Mapped[bool] = mapped_column(default=False)
    ip_address: Mapped[Optional[str]] = mapped_column(String(45), default=None)


class PaymentWebhook(PaymentWebhookBase, Base):
    __tablename__ = "payment_webhooks"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    received_at: Mapped[datetime] = mapped_column(default_factory=datetime.now)
