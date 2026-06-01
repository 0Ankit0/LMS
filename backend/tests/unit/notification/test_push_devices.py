import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core.config import settings

from .helpers import login, make_user


@pytest.mark.unit
class TestPushDevicesAPI:
    @pytest.fixture(autouse=True)
    def restore_push_settings(self):
        original = {
            "PUSH_ENABLED": settings.PUSH_ENABLED,
            "VAPID_PUBLIC_KEY": settings.VAPID_PUBLIC_KEY,
            "VAPID_PRIVATE_KEY": settings.VAPID_PRIVATE_KEY,
            "FCM_SERVER_KEY": settings.FCM_SERVER_KEY,
            "FCM_PROJECT_ID": settings.FCM_PROJECT_ID,
            "FCM_SERVICE_ACCOUNT_JSON": settings.FCM_SERVICE_ACCOUNT_JSON,
            "FCM_SERVICE_ACCOUNT_FILE": settings.FCM_SERVICE_ACCOUNT_FILE,
            "ONESIGNAL_APP_ID": settings.ONESIGNAL_APP_ID,
            "ONESIGNAL_API_KEY": settings.ONESIGNAL_API_KEY,
        }
        yield
        for key, value in original.items():
            setattr(settings, key, value)

    def _enable_webpush(self) -> None:
        settings.PUSH_ENABLED = True
        settings.VAPID_PUBLIC_KEY = "test-vapid-public"
        settings.VAPID_PRIVATE_KEY = "test-vapid-private"

    def _enable_fcm(self) -> None:
        settings.PUSH_ENABLED = True
        settings.FCM_SERVER_KEY = "test-fcm-server-key"

    def _enable_onesignal(self) -> None:
        settings.PUSH_ENABLED = True
        settings.ONESIGNAL_APP_ID = "test-onesignal-app-id"
        settings.ONESIGNAL_API_KEY = "test-onesignal-api-key"

    @pytest.mark.asyncio
    async def test_register_and_list_notification_devices(self, client: AsyncClient, db_session: AsyncSession):
        self._enable_webpush()
        await make_user(db_session, username="apiuser8", email="api8@example.com")
        token = await login(client, "apiuser8")

        create_resp = await client.post(
            "/api/v1/notifications/devices/",
            headers={"Authorization": f"Bearer {token}"},
            json={
                "provider": "webpush",
                "platform": "web",
                "endpoint": "https://push.example.com/subscription",
                "p256dh": "test-p256dh",
                "auth": "test-auth",
            },
        )
        assert create_resp.status_code == 201, create_resp.text
        assert create_resp.json()["provider"] == "webpush"

        list_resp = await client.get("/api/v1/notifications/devices/", headers={"Authorization": f"Bearer {token}"})
        assert list_resp.status_code == 200
        devices = list_resp.json()
        assert len(devices) == 1
        assert devices[0]["platform"] == "web"

    @pytest.mark.asyncio
    async def test_push_subscription_compatibility_wrapper_uses_device_registry(self, client: AsyncClient, db_session: AsyncSession):
        self._enable_webpush()
        await make_user(db_session, username="apiuser9", email="api9@example.com")
        token = await login(client, "apiuser9")

        put_resp = await client.put(
            "/api/v1/notifications/preferences/push-subscription/",
            headers={"Authorization": f"Bearer {token}"},
            json={
                "endpoint": "https://push.example.com/subscription-2",
                "p256dh": "compat-p256dh",
                "auth": "compat-auth",
            },
        )
        assert put_resp.status_code == 200, put_resp.text
        assert put_resp.json()["push_enabled"] is True

        devices_resp = await client.get("/api/v1/notifications/devices/", headers={"Authorization": f"Bearer {token}"})
        assert devices_resp.status_code == 200
        assert len(devices_resp.json()) == 1

        delete_resp = await client.delete(
            "/api/v1/notifications/preferences/push-subscription/",
            headers={"Authorization": f"Bearer {token}"},
        )
        assert delete_resp.status_code == 204

        devices_after = await client.get("/api/v1/notifications/devices/", headers={"Authorization": f"Bearer {token}"})
        assert devices_after.status_code == 200
        assert devices_after.json() == []

    @pytest.mark.asyncio
    async def test_register_fcm_device_with_provider_specific_endpoint(self, client: AsyncClient, db_session: AsyncSession):
        self._enable_fcm()
        await make_user(db_session, username="apiuser10", email="api10@example.com")
        token = await login(client, "apiuser10")

        resp = await client.post(
            "/api/v1/notifications/devices/fcm/",
            headers={"Authorization": f"Bearer {token}"},
            json={"platform": "android", "token": "fcm-device-token", "device_metadata": {"app_version": "1.0.0"}},
        )

        assert resp.status_code == 201, resp.text
        assert resp.json()["provider"] == "fcm"
        assert resp.json()["token"] == "fcm-device-token"

    @pytest.mark.asyncio
    async def test_register_onesignal_device_with_provider_specific_endpoint(self, client: AsyncClient, db_session: AsyncSession):
        self._enable_onesignal()
        await make_user(db_session, username="apiuser11", email="api11@example.com")
        token = await login(client, "apiuser11")

        resp = await client.post(
            "/api/v1/notifications/devices/onesignal/",
            headers={"Authorization": f"Bearer {token}"},
            json={"platform": "ios", "subscription_id": "onesignal-subscription-id"},
        )

        assert resp.status_code == 201, resp.text
        assert resp.json()["provider"] == "onesignal"
        assert resp.json()["subscription_id"] == "onesignal-subscription-id"

    @pytest.mark.asyncio
    async def test_push_endpoints_return_503_when_push_disabled(self, client: AsyncClient, db_session: AsyncSession):
        settings.PUSH_ENABLED = False
        await make_user(db_session, username="apiuser12", email="api12@example.com")
        token = await login(client, "apiuser12")

        resp = await client.get("/api/v1/notifications/push/config/", headers={"Authorization": f"Bearer {token}"})
        assert resp.status_code == 503
        assert "disabled" in resp.json()["detail"].lower()

    @pytest.mark.asyncio
    async def test_register_device_rejects_unconfigured_provider(self, client: AsyncClient, db_session: AsyncSession):
        self._enable_webpush()
        settings.FCM_SERVER_KEY = ""
        settings.FCM_PROJECT_ID = ""
        settings.FCM_SERVICE_ACCOUNT_JSON = ""
        settings.FCM_SERVICE_ACCOUNT_FILE = ""
        await make_user(db_session, username="apiuser13", email="api13@example.com")
        token = await login(client, "apiuser13")

        resp = await client.post(
            "/api/v1/notifications/devices/fcm/",
            headers={"Authorization": f"Bearer {token}"},
            json={"platform": "android", "token": "fcm-device-token"},
        )

        assert resp.status_code == 503
        assert "not configured" in resp.json()["detail"].lower()
