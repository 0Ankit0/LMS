import json
from unittest.mock import AsyncMock, MagicMock

import pytest

from src.apps.websocket.crypto import decrypt, derive_session_key
from src.apps.websocket.manager import ConnectionManager
from src.apps.websocket.schemas.messages import WSEncryptedFrame, WSEventMessage, WSPongMessage


class TestConnectionManager:
    def _make_ws(self) -> MagicMock:
        ws = MagicMock()
        ws.send_text = AsyncMock()
        ws.accept = AsyncMock()
        return ws

    def _make_key(self) -> bytes:
        return derive_session_key("test-jti")

    @pytest.mark.unit
    async def test_connect_increments_count(self):
        mgr = ConnectionManager()
        ws = self._make_ws()
        await mgr.connect(ws, user_id=1, session_key=self._make_key())
        assert mgr.total_connections == 1
        assert mgr.is_online(1)

    @pytest.mark.unit
    async def test_disconnect_removes_user(self):
        mgr = ConnectionManager()
        ws = self._make_ws()
        key = self._make_key()
        await mgr.connect(ws, user_id=1, session_key=key)
        await mgr.disconnect(ws, user_id=1)
        assert not mgr.is_online(1)
        assert mgr.total_connections == 0

    @pytest.mark.unit
    async def test_join_leave_room(self):
        mgr = ConnectionManager()
        ws = self._make_ws()
        key = self._make_key()
        await mgr.connect(ws, user_id=1, session_key=key)
        await mgr.join_room(1, "lobby")
        assert mgr.rooms_stats["lobby"] == 1
        await mgr.leave_room(1, "lobby")
        assert "lobby" not in mgr.rooms_stats

    @pytest.mark.unit
    async def test_send_personal_encrypts_frame(self):
        mgr = ConnectionManager()
        ws = self._make_ws()
        key = self._make_key()
        await mgr.connect(ws, user_id=1, session_key=key)

        await mgr.send_personal_model(1, WSPongMessage())

        raw = ws.send_text.call_args[0][0]
        frame = WSEncryptedFrame.model_validate_json(raw)
        inner = json.loads(decrypt(frame.iv, frame.data, key))
        assert inner["type"] == "pong"

    @pytest.mark.unit
    async def test_broadcast_room_sends_to_all_members(self):
        mgr = ConnectionManager()
        ws1 = self._make_ws()
        ws2 = self._make_ws()
        k1 = derive_session_key("jti-1")
        k2 = derive_session_key("jti-2")
        await mgr.connect(ws1, 1, k1)
        await mgr.connect(ws2, 2, k2)
        await mgr.join_room(1, "room-a")
        await mgr.join_room(2, "room-a")
        ws1.send_text.reset_mock()
        ws2.send_text.reset_mock()

        await mgr.broadcast_room("room-a", WSEventMessage(event="test.event", data={"x": 1}))

        for ws, key in [(ws1, k1), (ws2, k2)]:
            raw = ws.send_text.call_args[0][0]
            frame = WSEncryptedFrame.model_validate_json(raw)
            inner = json.loads(decrypt(frame.iv, frame.data, key))
            assert inner["event"] == "test.event"

    @pytest.mark.unit
    async def test_broadcast_room_exclude_sender(self):
        mgr = ConnectionManager()
        ws1 = self._make_ws()
        ws2 = self._make_ws()
        k1 = derive_session_key("jti-ex-1")
        k2 = derive_session_key("jti-ex-2")
        await mgr.connect(ws1, 1, k1)
        await mgr.connect(ws2, 2, k2)
        await mgr.join_room(1, "room-b")
        await mgr.join_room(2, "room-b")
        ws1.send_text.reset_mock()
        ws2.send_text.reset_mock()

        await mgr.broadcast_room("room-b", WSEventMessage(event="msg", data={}), exclude_user=1)

        ws1.send_text.assert_not_called()
        ws2.send_text.assert_called_once()

    @pytest.mark.unit
    async def test_users_online_list(self):
        mgr = ConnectionManager()
        await mgr.connect(self._make_ws(), 10, derive_session_key("j10"))
        await mgr.connect(self._make_ws(), 20, derive_session_key("j20"))
        assert set(mgr.users_online) == {10, 20}

    @pytest.mark.unit
    async def test_disconnect_removes_from_rooms(self):
        mgr = ConnectionManager()
        ws = self._make_ws()
        await mgr.connect(ws, 5, derive_session_key("jti-rm"))
        await mgr.join_room(5, "room-cleanup")
        await mgr.disconnect(ws, 5)
        assert "room-cleanup" not in mgr.rooms_stats

    @pytest.mark.unit
    async def test_push_event_helper(self):
        mgr = ConnectionManager()
        ws = self._make_ws()
        key = derive_session_key("jti-push")
        await mgr.connect(ws, 7, key)
        await mgr.push_event(7, "payment.completed", {"amount": 100})
        frame = WSEncryptedFrame.model_validate_json(ws.send_text.call_args[0][0])
        inner = json.loads(decrypt(frame.iv, frame.data, key))
        assert inner["event"] == "payment.completed"
