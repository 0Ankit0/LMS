from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core import security
from tests.factories import UserFactory


async def make_user(db: AsyncSession, **kwargs):
    defaults = dict(
        username="notifuser",
        email="notif@example.com",
        hashed_password=security.get_password_hash("TestPass123"),
        is_active=True,
        is_confirmed=True,
    )
    defaults.update(kwargs)
    user = UserFactory.build(**defaults)
    db.add(user)
    await db.commit()
    await db.refresh(user)
    return user


async def login(client: AsyncClient, username: str, password: str = "TestPass123") -> str:
    resp = await client.post(
        "/api/v1/auth/login/?set_cookie=false",
        json={"username": username, "password": password},
    )
    assert resp.status_code == 200, resp.text
    return resp.json()["access"]
