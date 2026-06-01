import json
from unittest.mock import AsyncMock, MagicMock

from fastapi.websockets import WebSocketDisconnect
from httpx import AsyncClient
from jose import jwt
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core import security
from src.apps.core.config import settings
from src.apps.core.security import ALGORITHM, TokenType
from src.apps.iam.models.token_tracking import TokenTracking
from src.apps.websocket.crypto import derive_session_key, encrypt
from src.apps.websocket.schemas.messages import WSEncryptedFrame
from tests.factories import UserFactory


async def make_auth_user(db_session: AsyncSession):
    user = UserFactory.build(
        username="wsuser",
        email="ws@example.com",
        hashed_password=security.get_password_hash("Pass123!"),
        is_active=True,
    )
    db_session.add(user)
    await db_session.commit()
    await db_session.refresh(user)
    return user


async def get_token(client: AsyncClient, db_session: AsyncSession):
    user = await make_auth_user(db_session)
    resp = await client.post(
        "/api/v1/auth/login/?set_cookie=false",
        json={"username": "wsuser", "password": "Pass123!"},
    )
    return resp.json().get("access", ""), user


async def setup_user_and_token(db_session: AsyncSession):
    from datetime import datetime, timedelta, timezone
    import uuid

    user = UserFactory.build(
        username="wshandshake",
        email="wshandshake@example.com",
        hashed_password=security.get_password_hash("Pass123!"),
        is_active=True,
    )
    db_session.add(user)
    await db_session.commit()
    await db_session.refresh(user)

    jti = str(uuid.uuid4())
    expires = datetime.now(timezone.utc) + timedelta(hours=1)
    token = jwt.encode(
        {"sub": str(user.id), "jti": jti, "exp": expires, "type": "access"},
        settings.SECRET_KEY,
        algorithm=ALGORITHM,
    )

    tracking = TokenTracking(
        user_id=user.id,
        token_jti=jti,
        token_type=TokenType.ACCESS,
        ip_address="127.0.0.1",
        user_agent="test",
        expires_at=expires,
        is_active=True,
    )
    db_session.add(tracking)
    await db_session.commit()
    return user, token, jti


def make_mock_ws(token: str, texts_to_receive: list[str]) -> MagicMock:
    ws = MagicMock()
    ws.accept = AsyncMock()
    ws.close = AsyncMock()
    ws.send_json = AsyncMock()
    ws.send_text = AsyncMock()
    ws.query_params = {"token": token}
    ws.headers = {}

    receive_calls = iter(texts_to_receive)

    async def _recv():
        try:
            return next(receive_calls)
        except StopIteration:
            raise WebSocketDisconnect()

    ws.receive_text = _recv
    return ws


def make_encrypted_frame(jti: str, payload: dict) -> str:
    key = derive_session_key(jti)
    plaintext = json.dumps(payload).encode()
    iv_b64, ct_b64 = encrypt(plaintext, key)
    return WSEncryptedFrame(type=payload["type"], iv=iv_b64, data=ct_b64).model_dump_json()
