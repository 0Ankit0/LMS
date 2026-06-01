import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.iam.utils.hashid import encode_id

from .helpers import get_token


class TestWSRestEndpoints:
    @pytest.mark.unit
    async def test_stats_requires_auth(self, client: AsyncClient):
        resp = await client.get("/api/v1/ws/stats/")
        assert resp.status_code == 401

    @pytest.mark.unit
    async def test_online_requires_auth(self, client: AsyncClient):
        resp = await client.get("/api/v1/ws/online/1/")
        assert resp.status_code == 401

    @pytest.mark.unit
    async def test_stats_returns_structure(self, client: AsyncClient, db_session: AsyncSession):
        token, _ = await get_token(client, db_session)
        resp = await client.get("/api/v1/ws/stats/", headers={"Authorization": f"Bearer {token}"})
        assert resp.status_code == 200
        data = resp.json()
        assert "total_connections" in data
        assert "rooms" in data
        assert "users_online" in data

    @pytest.mark.unit
    async def test_online_check(self, client: AsyncClient, db_session: AsyncSession):
        token, user = await get_token(client, db_session)
        resp = await client.get(
            f"/api/v1/ws/online/{encode_id(user.id)}/",
            headers={"Authorization": f"Bearer {token}"},
        )
        assert resp.status_code == 200
        data = resp.json()
        assert data["user_id"] == encode_id(user.id)
        assert data["online"] is False
