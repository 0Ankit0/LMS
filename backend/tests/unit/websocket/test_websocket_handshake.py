import base64
import json
from unittest.mock import AsyncMock, MagicMock

import pytest
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.websocket.crypto import decrypt, derive_session_key
from src.apps.websocket.schemas.messages import WSEncryptedFrame

from .helpers import make_encrypted_frame, make_mock_ws, setup_user_and_token


class TestWSHandshakeAndMessages:
    @pytest.mark.unit
    async def test_handshake_frame_sent_on_connect(self, db_session: AsyncSession):
        _user, token, _jti = await setup_user_and_token(db_session)
        ws = make_mock_ws(token, [])

        from src.apps.websocket.api.v1.ws import _handle_connection

        await _handle_connection(ws, db_session, initial_room=None)

        first_call = ws.send_text.call_args_list[0][0][0]
        handshake = json.loads(first_call)
        assert handshake["type"] == "handshake"
        assert len(base64.b64decode(handshake["session_key"])) == 32

    @pytest.mark.unit
    async def test_ping_returns_encrypted_pong(self, db_session: AsyncSession):
        _user, token, jti = await setup_user_and_token(db_session)
        ws = make_mock_ws(token, [make_encrypted_frame(jti, {"type": "ping"})])

        from src.apps.websocket.api.v1.ws import _handle_connection

        await _handle_connection(ws, db_session, initial_room=None)

        calls = [c[0][0] for c in ws.send_text.call_args_list]
        key = derive_session_key(jti)
        pong_frames = []
        for raw in calls[1:]:
            try:
                frame = WSEncryptedFrame.model_validate_json(raw)
                inner = json.loads(decrypt(frame.iv, frame.data, key))
                if inner.get("type") == "pong":
                    pong_frames.append(inner)
            except Exception:
                continue

        assert len(pong_frames) == 1

    @pytest.mark.unit
    async def test_invalid_token_closes_with_4001(self, db_session: AsyncSession):
        ws = MagicMock()
        ws.accept = AsyncMock()
        ws.close = AsyncMock()
        ws.send_json = AsyncMock()
        ws.send_text = AsyncMock()
        ws.query_params = {"token": "invalid.jwt.token"}
        ws.headers = {}

        from src.apps.websocket.api.v1.ws import _handle_connection

        await _handle_connection(ws, db_session, initial_room=None)
        ws.close.assert_called_once_with(code=4001)

    @pytest.mark.unit
    async def test_missing_token_closes_with_4001(self, db_session: AsyncSession):
        ws = MagicMock()
        ws.accept = AsyncMock()
        ws.close = AsyncMock()
        ws.send_json = AsyncMock()
        ws.send_text = AsyncMock()
        ws.query_params = {}
        ws.headers = {}

        from src.apps.websocket.api.v1.ws import _handle_connection

        await _handle_connection(ws, db_session, initial_room=None)
        ws.close.assert_called_once_with(code=4001)
