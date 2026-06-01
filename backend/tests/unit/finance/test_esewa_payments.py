import base64
import json
from unittest.mock import AsyncMock, MagicMock, patch

import pytest
from httpx import AsyncClient, Response
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.finance.models.payment import PaymentProvider, PaymentStatus, PaymentTransaction
from src.apps.iam.utils.hashid import encode_id

from .helpers import esewa_callback_data, esewa_sig, esewa_signed_message


class TestEsewaPayment:
    @pytest.mark.unit
    async def test_esewa_initiate_success(self, client: AsyncClient, db_session: AsyncSession):
        resp = await client.post(
            "/api/v1/payments/initiate/",
            json={
                "provider": "esewa",
                "amount": 100,
                "purchase_order_id": "ESEWA-ORDER-001",
                "purchase_order_name": "Test eSewa Order",
                "return_url": "http://localhost:3000/payment/esewa/callback",
            },
        )

        assert resp.status_code == 200
        data = resp.json()
        assert data["provider"] == "esewa"
        assert data["status"] == "initiated"
        assert data["payment_url"] is not None
        assert "form_fields" in data["extra"]

        form_fields = data["extra"]["form_fields"]
        assert form_fields["product_code"] == "EPAYTEST"
        assert form_fields["total_amount"] == 100
        assert form_fields["signed_field_names"] == "total_amount,transaction_uuid,product_code"
        assert "signature" in form_fields

        message = esewa_signed_message(
            form_fields["signed_field_names"],
            {
                "total_amount": str(form_fields["total_amount"]),
                "transaction_uuid": form_fields["transaction_uuid"],
                "product_code": form_fields["product_code"],
            },
        )
        assert form_fields["signature"] == esewa_sig(message)

    @pytest.mark.unit
    async def test_esewa_verify_success(self, client: AsyncClient, db_session: AsyncSession):
        transaction_uuid = "esewa-uuid-test-001"
        tx = PaymentTransaction(
            provider=PaymentProvider.ESEWA,
            amount=100,
            purchase_order_id="ESEWA-ORDER-001",
            purchase_order_name="Test eSewa Order",
            return_url="http://localhost:3000/callback",
            website_url="",
            status=PaymentStatus.INITIATED,
            provider_pidx=transaction_uuid,
        )
        db_session.add(tx)
        await db_session.commit()
        await db_session.refresh(tx)

        mock_response = MagicMock(spec=Response)
        mock_response.status_code = 200
        mock_response.json.return_value = {
            "product_code": "EPAYTEST",
            "transaction_uuid": transaction_uuid,
            "total_amount": "100",
            "status": "COMPLETE",
            "ref_id": "ESEWA_REF_001",
        }

        mock_get_client = MagicMock()
        mock_get_client.__aenter__ = AsyncMock(return_value=mock_get_client)
        mock_get_client.__aexit__ = AsyncMock(return_value=False)
        mock_get_client.get = AsyncMock(return_value=mock_response)

        with patch("src.apps.finance.services.esewa.httpx.AsyncClient", return_value=mock_get_client):
            resp = await client.post(
                "/api/v1/payments/verify/",
                json={"provider": "esewa", "data": esewa_callback_data(transaction_uuid, total_amount=100)},
            )

        assert resp.status_code == 200
        data = resp.json()
        assert data["status"] == "completed"
        assert data["transaction_id"] == encode_id(tx.id)

    @pytest.mark.unit
    async def test_esewa_verify_invalid_signature(self, client: AsyncClient, db_session: AsyncSession):
        bad_payload = base64.b64encode(json.dumps({
            "transaction_code": "X",
            "status": "COMPLETE",
            "total_amount": "100",
            "transaction_uuid": "uuid-tampered",
            "product_code": "EPAYTEST",
            "signed_field_names": "transaction_code,status,total_amount,transaction_uuid,product_code,signed_field_names",
            "signature": "INVALIDSIGNATURE==",
        }).encode()).decode()

        resp = await client.post("/api/v1/payments/verify/", json={"provider": "esewa", "data": bad_payload})
        assert resp.status_code == 400

    @pytest.mark.unit
    async def test_esewa_verify_missing_data(self, client: AsyncClient):
        resp = await client.post("/api/v1/payments/verify/", json={"provider": "esewa"})
        assert resp.status_code == 400
