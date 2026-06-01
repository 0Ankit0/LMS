from unittest.mock import MagicMock, patch

import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.finance.models.payment import PaymentProvider, PaymentStatus, PaymentTransaction
from src.apps.iam.utils.hashid import encode_id


class TestTransactionCRUD:
    @pytest.mark.unit
    async def test_get_transaction_not_found(self, client: AsyncClient):
        resp = await client.get(f"/api/v1/payments/{encode_id(99999)}/")
        assert resp.status_code == 404

    @pytest.mark.unit
    async def test_get_transaction_success(self, client: AsyncClient, db_session: AsyncSession):
        tx = PaymentTransaction(
            provider=PaymentProvider.KHALTI,
            amount=500,
            purchase_order_id="TX-READ-001",
            purchase_order_name="Read Test",
            return_url="http://localhost:3000/cb",
            website_url="",
            status=PaymentStatus.COMPLETED,
        )
        db_session.add(tx)
        await db_session.commit()
        await db_session.refresh(tx)

        resp = await client.get(f"/api/v1/payments/{encode_id(tx.id)}/")
        assert resp.status_code == 200
        data = resp.json()
        assert data["purchase_order_id"] == "TX-READ-001"
        assert data["status"] == "completed"

    @pytest.mark.unit
    async def test_list_transactions(self, client: AsyncClient, db_session: AsyncSession):
        for i in range(3):
            db_session.add(PaymentTransaction(
                provider=PaymentProvider.ESEWA,
                amount=100 * (i + 1),
                purchase_order_id=f"LIST-{i}",
                purchase_order_name=f"Order {i}",
                return_url="http://localhost/cb",
                website_url="",
                status=PaymentStatus.PENDING,
            ))
        await db_session.commit()

        resp = await client.get("/api/v1/payments/?provider=esewa&limit=10")
        assert resp.status_code == 200
        assert len(resp.json()) >= 3

    @pytest.mark.unit
    async def test_initiate_invalid_amount(self, client: AsyncClient):
        resp = await client.post(
            "/api/v1/payments/initiate/",
            json={
                "provider": "khalti",
                "amount": 0,
                "purchase_order_id": "BAD",
                "purchase_order_name": "Bad",
                "return_url": "http://localhost/cb",
            },
        )
        assert resp.status_code == 422

    @pytest.mark.unit
    async def test_unsupported_provider(self, client: AsyncClient):
        resp = await client.post(
            "/api/v1/payments/initiate/",
            json={
                "provider": "stripe",
                "amount": 1000,
                "purchase_order_id": "STR-001",
                "purchase_order_name": "Stripe Order",
                "return_url": "http://localhost/cb",
            },
        )
        assert resp.status_code == 503


class TestProviderFlags:
    @pytest.mark.unit
    async def test_list_enabled_providers(self, client: AsyncClient):
        resp = await client.get("/api/v1/payments/providers/")
        assert resp.status_code == 200
        providers = resp.json()
        assert "khalti" in providers
        assert "esewa" in providers
        assert "stripe" not in providers
        assert "paypal" not in providers

    @pytest.mark.unit
    async def test_disabled_provider_returns_503(self, client: AsyncClient):
        for provider in ("stripe", "paypal"):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": provider,
                    "amount": 1000,
                    "purchase_order_id": f"{provider}-order",
                    "purchase_order_name": "Test",
                    "return_url": "http://localhost/cb",
                },
            )
            assert resp.status_code == 503

    @pytest.mark.unit
    async def test_stripe_enabled_flag(self, client: AsyncClient, db_session: AsyncSession):
        mock_session = MagicMock()
        mock_session.id = "cs_test_abc123"
        mock_session.url = "https://checkout.stripe.com/pay/cs_test_abc123"
        mock_session.payment_status = "unpaid"

        import src.apps.finance.api.v1.payment as payment_module
        from src.apps.finance.services.stripe import StripeService

        original = dict(payment_module._PROVIDERS)
        payment_module._PROVIDERS[PaymentProvider.STRIPE] = StripeService()

        with patch(
            "src.apps.finance.services.stripe.stripe.checkout.Session.create",
            return_value=mock_session,
        ):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": "stripe",
                    "amount": 1000,
                    "purchase_order_id": "STR-ENABLED-001",
                    "purchase_order_name": "Stripe Test",
                    "return_url": "http://localhost/cb",
                },
            )

        payment_module._PROVIDERS.clear()
        payment_module._PROVIDERS.update(original)

        assert resp.status_code == 200
        data = resp.json()
        assert data["provider"] == "stripe"
        assert data["provider_pidx"] == "cs_test_abc123"

    @pytest.mark.unit
    async def test_paypal_enabled_flag(self, client: AsyncClient, db_session: AsyncSession):
        mock_payment = MagicMock()
        mock_payment.id = "PAY-test123"
        mock_payment.state = "created"
        mock_payment.error = None
        mock_link = MagicMock()
        mock_link.rel = "approval_url"
        mock_link.href = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token=EC-test"
        mock_payment.links = [mock_link]
        mock_payment.create.return_value = True

        import src.apps.finance.api.v1.payment as payment_module
        from src.apps.finance.services.paypal import PayPalService

        original = dict(payment_module._PROVIDERS)
        payment_module._PROVIDERS[PaymentProvider.PAYPAL] = PayPalService()

        with patch(
            "src.apps.finance.services.paypal.paypalrestsdk.Payment",
            return_value=mock_payment,
        ):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": "paypal",
                    "amount": 1000,
                    "purchase_order_id": "PP-ENABLED-001",
                    "purchase_order_name": "PayPal Test",
                    "return_url": "http://localhost/cb",
                },
            )

        payment_module._PROVIDERS.clear()
        payment_module._PROVIDERS.update(original)

        assert resp.status_code == 200
        data = resp.json()
        assert data["provider"] == "paypal"
        assert data["provider_pidx"] == "PAY-test123"
