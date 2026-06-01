from unittest.mock import AsyncMock, patch

import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.iam.utils.hashid import encode_id
from src.apps.notification.models.notification import Notification

from .helpers import login, make_user


@pytest.mark.unit
class TestNotificationAPI:
    @pytest.mark.asyncio
    async def test_list_notifications_empty(self, client: AsyncClient, db_session: AsyncSession):
        await make_user(db_session, username="apiuser1", email="api1@example.com")
        token = await login(client, "apiuser1")
        resp = await client.get("/api/v1/notifications/", headers={"Authorization": f"Bearer {token}"})
        assert resp.status_code == 200
        data = resp.json()
        assert data["total"] == 0
        assert data["unread_count"] == 0

    @pytest.mark.asyncio
    async def test_list_notifications_with_data(self, client: AsyncClient, db_session: AsyncSession):
        user = await make_user(db_session, username="apiuser2", email="api2@example.com")
        db_session.add(Notification(user_id=user.id, title="A", body="B"))
        db_session.add(Notification(user_id=user.id, title="C", body="D"))
        await db_session.commit()

        token = await login(client, "apiuser2")
        resp = await client.get("/api/v1/notifications/", headers={"Authorization": f"Bearer {token}"})
        assert resp.status_code == 200
        assert resp.json()["total"] == 2

    @pytest.mark.asyncio
    async def test_mark_single_read(self, client: AsyncClient, db_session: AsyncSession):
        user = await make_user(db_session, username="apiuser3", email="api3@example.com")
        n = Notification(user_id=user.id, title="T", body="B")
        db_session.add(n)
        await db_session.commit()
        await db_session.refresh(n)

        token = await login(client, "apiuser3")
        resp = await client.patch(
            f"/api/v1/notifications/{encode_id(n.id)}/read/",
            headers={"Authorization": f"Bearer {token}"},
        )
        assert resp.status_code == 200
        assert resp.json()["is_read"] is True

    @pytest.mark.asyncio
    async def test_mark_all_read_endpoint(self, client: AsyncClient, db_session: AsyncSession):
        user = await make_user(db_session, username="apiuser4", email="api4@example.com")
        for _ in range(3):
            db_session.add(Notification(user_id=user.id, title="X", body="Y"))
        await db_session.commit()

        token = await login(client, "apiuser4")
        resp = await client.patch("/api/v1/notifications/read-all/", headers={"Authorization": f"Bearer {token}"})
        assert resp.status_code == 200
        assert resp.json()["updated"] == 3

    @pytest.mark.asyncio
    async def test_delete_notification_endpoint(self, client: AsyncClient, db_session: AsyncSession):
        user = await make_user(db_session, username="apiuser5", email="api5@example.com")
        n = Notification(user_id=user.id, title="Del", body="Me")
        db_session.add(n)
        await db_session.commit()
        await db_session.refresh(n)

        token = await login(client, "apiuser5")
        resp = await client.delete(
            f"/api/v1/notifications/{encode_id(n.id)}/",
            headers={"Authorization": f"Bearer {token}"},
        )
        assert resp.status_code == 204

    @pytest.mark.asyncio
    async def test_delete_notification_not_found(self, client: AsyncClient, db_session: AsyncSession):
        await make_user(db_session, username="apiuser6", email="api6@example.com")
        token = await login(client, "apiuser6")
        resp = await client.delete(
            f"/api/v1/notifications/{encode_id(99999)}/",
            headers={"Authorization": f"Bearer {token}"},
        )
        assert resp.status_code == 404

    @pytest.mark.asyncio
    async def test_create_notification_requires_superuser(self, client: AsyncClient, db_session: AsyncSession):
        user = await make_user(db_session, username="apiuser7", email="api7@example.com")

        token = await login(client, "apiuser7")
        resp = await client.post(
            "/api/v1/notifications/",
            json={"user_id": encode_id(user.id), "title": "T", "body": "B", "type": "info"},
            headers={"Authorization": f"Bearer {token}"},
        )
        assert resp.status_code == 403

    @pytest.mark.asyncio
    async def test_create_notification_as_superuser(self, client: AsyncClient, db_session: AsyncSession):
        target = await make_user(db_session, username="target1", email="target1@example.com")
        await make_user(db_session, username="suadmin", email="suadmin@example.com", is_superuser=True)
        token = await login(client, "suadmin")

        with patch("src.apps.notification.services.notification._push_to_ws", new_callable=AsyncMock):
            resp = await client.post(
                "/api/v1/notifications/",
                json={"user_id": encode_id(target.id), "title": "Hi", "body": "There", "type": "success"},
                headers={"Authorization": f"Bearer {token}"},
            )
        assert resp.status_code == 201
        data = resp.json()
        assert data["title"] == "Hi"
        assert data["type"] == "success"
