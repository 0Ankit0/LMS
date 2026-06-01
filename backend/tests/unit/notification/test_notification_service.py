from unittest.mock import AsyncMock, patch

import pytest
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.notification.models.notification import Notification, NotificationType
from src.apps.notification.schemas.notification import NotificationCreate
from src.apps.notification.services.notification import (
    create_notification,
    delete_notification,
    get_notification,
    get_user_notifications,
    mark_all_read,
    mark_as_read,
)

from .helpers import make_user


@pytest.mark.unit
class TestNotificationService:
    @pytest.mark.asyncio
    async def test_create_notification_persisted(self, db_session: AsyncSession):
        user = await make_user(db_session)
        data = NotificationCreate(user_id=user.id, title="Hello", body="World", type=NotificationType.INFO)
        with patch("src.apps.notification.services.notification._push_to_ws", new_callable=AsyncMock) as mock_push:
            notification = await create_notification(db_session, data, push_ws=True)

        assert notification.id is not None
        assert notification.title == "Hello"
        assert notification.is_read is False
        mock_push.assert_awaited_once()

    @pytest.mark.asyncio
    async def test_create_notification_no_ws(self, db_session: AsyncSession):
        user = await make_user(db_session, username="notifuser2", email="notif2@example.com")
        data = NotificationCreate(user_id=user.id, title="T", body="B")
        notification = await create_notification(db_session, data, push_ws=False)
        assert notification.id is not None

    @pytest.mark.asyncio
    async def test_get_user_notifications(self, db_session: AsyncSession):
        user = await make_user(db_session, username="notifuser3", email="notif3@example.com")
        for i in range(3):
            db_session.add(Notification(user_id=user.id, title=f"N{i}", body="body"))
        await db_session.commit()

        result = await get_user_notifications(db_session, user.id, limit=10)
        assert result.total == 3
        assert result.unread_count == 3
        assert len(result.items) == 3

    @pytest.mark.asyncio
    async def test_mark_as_read(self, db_session: AsyncSession):
        user = await make_user(db_session, username="notifuser4", email="notif4@example.com")
        n = Notification(user_id=user.id, title="T", body="B")
        db_session.add(n)
        await db_session.commit()
        await db_session.refresh(n)

        updated = await mark_as_read(db_session, n.id, user.id)
        assert updated is not None
        assert updated.is_read is True

    @pytest.mark.asyncio
    async def test_mark_all_read(self, db_session: AsyncSession):
        user = await make_user(db_session, username="notifuser5", email="notif5@example.com")
        for _ in range(4):
            db_session.add(Notification(user_id=user.id, title="T", body="B"))
        await db_session.commit()

        count = await mark_all_read(db_session, user.id)
        assert count == 4

        result = await get_user_notifications(db_session, user.id, unread_only=True)
        assert result.total == 0

    @pytest.mark.asyncio
    async def test_delete_notification(self, db_session: AsyncSession):
        user = await make_user(db_session, username="notifuser6", email="notif6@example.com")
        n = Notification(user_id=user.id, title="T", body="B")
        db_session.add(n)
        await db_session.commit()
        await db_session.refresh(n)

        deleted = await delete_notification(db_session, n.id, user.id)
        assert deleted is True
        assert await get_notification(db_session, n.id, user.id) is None

    @pytest.mark.asyncio
    async def test_delete_notification_not_found(self, db_session: AsyncSession):
        user = await make_user(db_session, username="notifuser7", email="notif7@example.com")
        assert await delete_notification(db_session, 99999, user.id) is False
