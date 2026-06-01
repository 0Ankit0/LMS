import json
from unittest.mock import AsyncMock, MagicMock, patch

import httpx
import pytest
from httpx import AsyncClient, Response
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.finance.models.payment import PaymentProvider, PaymentStatus, PaymentTransaction
from src.apps.iam.utils.hashid import encode_id


class TestKhaltiPayment:
    @pytest.mark.unit
    async def test_khalti_initiate_success(self, client: AsyncClient, db_session: AsyncSession):
        mock_response = MagicMock(spec=Response)
        mock_response.status_code = 200
        mock_response.json.return_value = {
            "pidx": "test_pidx_abc123",
            "payment_url": "https://test-pay.khalti.com/?pidx=test_pidx_abc123",
            "expires_at": "2024-12-31T23:59:59",
            "expires_in": 1800,
        }
        mock_response.text = json.dumps(mock_response.json.return_value)

        mock_client = MagicMock()
        mock_client.__aenter__ = AsyncMock(return_value=mock_client)
        mock_client.__aexit__ = AsyncMock(return_value=False)
        mock_client.post = AsyncMock(return_value=mock_response)

        with patch("src.apps.finance.services.khalti.httpx.AsyncClient", return_value=mock_client):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": "khalti",
                    "amount": 1000,
                    "purchase_order_id": "ORDER-001",
                    "purchase_order_name": "Test Order",
                    "return_url": "http://localhost:3000/payment/callback",
                    "website_url": "http://localhost:3000",
                },
            )

        assert resp.status_code == 200
        data = resp.json()
        assert data["provider"] == "khalti"
        assert data["status"] == "initiated"
        assert data["payment_url"] == "https://test-pay.khalti.com/?pidx=test_pidx_abc123"
        assert data["provider_pidx"] == "test_pidx_abc123"
        assert data["transaction_id"] is not None

    @pytest.mark.unit
    async def test_khalti_initiate_includes_phone_customer_info(self, client: AsyncClient):
        mock_response = MagicMock(spec=Response)
        mock_response.status_code = 200
        mock_response.json.return_value = {
            "pidx": "test_pidx_customer_001",
            "payment_url": "https://test-pay.khalti.com/?pidx=test_pidx_customer_001",
        }
        mock_response.text = json.dumps(mock_response.json.return_value)

        mock_client = MagicMock()
        mock_client.__aenter__ = AsyncMock(return_value=mock_client)
        mock_client.__aexit__ = AsyncMock(return_value=False)
        mock_client.post = AsyncMock(return_value=mock_response)

        with patch("src.apps.finance.services.khalti.httpx.AsyncClient", return_value=mock_client):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": "khalti",
                    "amount": 1000,
                    "purchase_order_id": "ORDER-CUSTOMER-001",
                    "purchase_order_name": "Customer Order",
                    "return_url": "http://localhost:3000/payment/callback",
                    "website_url": "http://localhost:3000",
                    "customer_name": "Test User",
                    "customer_email": "test@example.com",
                    "customer_phone": "9800000000",
                },
            )

        assert resp.status_code == 200
        _, kwargs = mock_client.post.await_args
        assert kwargs["json"]["customer_info"] == {
            "name": "Test User",
            "email": "test@example.com",
            "phone": "9800000000",
        }
        assert kwargs["headers"]["Authorization"].startswith("Key ")

    @pytest.mark.unit
    async def test_khalti_initiate_transport_error_has_message(self, client: AsyncClient):
        request = httpx.Request("POST", "https://dev.khalti.com/api/v2/epayment/initiate/")
        mock_client = MagicMock()
        mock_client.__aenter__ = AsyncMock(return_value=mock_client)
        mock_client.__aexit__ = AsyncMock(return_value=False)
        mock_client.post = AsyncMock(side_effect=httpx.ReadError("", request=request))

        with patch("src.apps.finance.services.khalti.httpx.AsyncClient", return_value=mock_client):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": "khalti",
                    "amount": 1000,
                    "purchase_order_id": "ORDER-ERR-READ",
                    "purchase_order_name": "Read Error Order",
                    "return_url": "http://localhost:3000/payment/callback",
                },
            )

        assert resp.status_code == 502
        assert "Khalti initiation request failed" in resp.json()["detail"]
        assert "configured host" in resp.json()["detail"]
        assert "ReadError" in resp.json()["detail"]

    @pytest.mark.unit
    async def test_khalti_initiate_minimum_amount_guard(self, client: AsyncClient):
        resp = await client.post(
            "/api/v1/payments/initiate/",
            json={
                "provider": "khalti",
                "amount": 999,
                "purchase_order_id": "ORDER-LOW-AMOUNT",
                "purchase_order_name": "Low Amount Order",
                "return_url": "http://localhost:3000/payment/callback",
            },
        )

        assert resp.status_code == 400
        assert "at least 1000 paisa" in resp.json()["detail"]

    @pytest.mark.unit
    async def test_khalti_initiate_provider_error(self, client: AsyncClient):
        mock_response = MagicMock(spec=Response)
        mock_response.status_code = 400
        mock_response.text = '{"detail": "Invalid amount"}'

        mock_client = MagicMock()
        mock_client.__aenter__ = AsyncMock(return_value=mock_client)
        mock_client.__aexit__ = AsyncMock(return_value=False)
        mock_client.post = AsyncMock(return_value=mock_response)

        with patch("src.apps.finance.services.khalti.httpx.AsyncClient", return_value=mock_client):
            resp = await client.post(
                "/api/v1/payments/initiate/",
                json={
                    "provider": "khalti",
                    "amount": 1000,
                    "purchase_order_id": "ORDER-ERR",
                    "purchase_order_name": "Bad Order",
                    "return_url": "http://localhost:3000/callback",
                },
            )

        assert resp.status_code == 400

    @pytest.mark.unit
    async def test_khalti_verify_success(self, client: AsyncClient, db_session: AsyncSession):
        tx = PaymentTransaction(
            provider=PaymentProvider.KHALTI,
            amount=1000,
            purchase_order_id="ORDER-001",
            purchase_order_name="Test Order",
            return_url="http://localhost:3000/callback",
            website_url="http://localhost:3000",
            status=PaymentStatus.INITIATED,
            provider_pidx="test_pidx_abc123",
        )
        db_session.add(tx)
        await db_session.commit()
        await db_session.refresh(tx)

        mock_response = MagicMock(spec=Response)
        mock_response.status_code = 200
        mock_response.json.return_value = {
            "pidx": "test_pidx_abc123",
            "total_amount": 1000,
            "status": "Completed",
            "transaction_id": "KHALTI_TXN_XYZ",
            "fee": 30,
            "refunded": False,
        }

        mock_client = MagicMock()
        mock_client.__aenter__ = AsyncMock(return_value=mock_client)
        mock_client.__aexit__ = AsyncMock(return_value=False)
        mock_client.post = AsyncMock(return_value=mock_response)

        with patch("src.apps.finance.services.khalti.httpx.AsyncClient", return_value=mock_client):
            resp = await client.post(
                "/api/v1/payments/verify/",
                json={"provider": "khalti", "pidx": "test_pidx_abc123"},
            )

        assert resp.status_code == 200
        data = resp.json()
        assert data["status"] == "completed"
        assert data["provider_transaction_id"] == "KHALTI_TXN_XYZ"
        assert data["transaction_id"] == encode_id(tx.id)

    @pytest.mark.unit
    async def test_khalti_verify_missing_pidx(self, client: AsyncClient):
        resp = await client.post("/api/v1/payments/verify/", json={"provider": "khalti"})
        assert resp.status_code == 400

    @pytest.mark.unit
    async def test_khalti_verify_cancelled(self, client: AsyncClient, db_session: AsyncSession):
        tx = PaymentTransaction(
            provider=PaymentProvider.KHALTI,
            amount=500,
            purchase_order_id="ORDER-CANCEL",
            purchase_order_name="Cancelled Order",
            return_url="http://localhost:3000/callback",
            website_url="",
            status=PaymentStatus.INITIATED,
            provider_pidx="pidx_cancel",
        )
        db_session.add(tx)
        await db_session.commit()
        await db_session.refresh(tx)

        mock_response = MagicMock(spec=Response)
        mock_response.status_code = 200
        mock_response.json.return_value = {
            "pidx": "pidx_cancel",
            "total_amount": 500,
            "status": "User canceled",
            "transaction_id": None,
            "fee": 0,
            "refunded": False,
        }

        mock_client = MagicMock()
        mock_client.__aenter__ = AsyncMock(return_value=mock_client)
        mock_client.__aexit__ = AsyncMock(return_value=False)
        mock_client.post = AsyncMock(return_value=mock_response)

        with patch("src.apps.finance.services.khalti.httpx.AsyncClient", return_value=mock_client):
            resp = await client.post(
                "/api/v1/payments/verify/",
                json={"provider": "khalti", "pidx": "pidx_cancel"},
            )

        assert resp.status_code == 200
        assert resp.json()["status"] == "cancelled"
