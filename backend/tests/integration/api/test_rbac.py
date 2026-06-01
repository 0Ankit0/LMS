import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core import security
from src.apps.iam.models.user import User
from src.apps.iam.utils.hashid import encode_id


async def _make_user(db: AsyncSession, **kwargs) -> User:
    user = User(
        username=kwargs.get("username", "user"),
        email=kwargs.get("email", "user@example.com"),
        hashed_password=security.get_password_hash(kwargs.get("password", "TestPass123")),
        is_active=kwargs.get("is_active", True),
        is_superuser=kwargs.get("is_superuser", False),
        is_confirmed=kwargs.get("is_confirmed", True),
    )
    db.add(user)
    await db.commit()
    await db.refresh(user)
    return user


async def _login(client: AsyncClient, username: str, password: str = "TestPass123") -> str:
    response = await client.post(
        "/api/v1/auth/login/?set_cookie=false",
        json={"username": username, "password": password},
    )
    assert response.status_code == 200, response.text
    return response.json()["access"]


@pytest.mark.integration
class TestRbacAPI:
    @pytest.mark.asyncio
    async def test_role_assignment_grants_casbin_permission(
        self,
        client: AsyncClient,
        db_session: AsyncSession,
    ):
        admin = await _make_user(
            db_session,
            username="rbacadmin",
            email="rbacadmin@example.com",
            is_superuser=True,
        )
        member = await _make_user(
            db_session,
            username="rbacmember",
            email="rbacmember@example.com",
            is_superuser=False,
        )
        admin_token = await _login(client, admin.username)
        headers = {"Authorization": f"Bearer {admin_token}"}

        role_response = await client.post(
            "/api/v1/roles",
            headers=headers,
            json={"name": "reports_viewer", "description": "Can view reports"},
        )
        assert role_response.status_code == 201, role_response.text
        role_id = role_response.json()["id"]

        permission_response = await client.post(
            "/api/v1/permissions",
            headers=headers,
            json={"resource": "reports", "action": "read", "description": "Read report data"},
        )
        assert permission_response.status_code == 201, permission_response.text
        permission_id = permission_response.json()["id"]

        assign_permission_response = await client.post(
            "/api/v1/roles/assign-permission",
            headers=headers,
            json={"role_id": role_id, "permission_id": permission_id},
        )
        assert assign_permission_response.status_code == 200, assign_permission_response.text

        assign_role_response = await client.post(
            "/api/v1/users/assign-role",
            headers=headers,
            json={"user_id": encode_id(member.id), "role_id": role_id},
        )
        assert assign_role_response.status_code == 200, assign_role_response.text

        user_roles_response = await client.get(
            f"/api/v1/users/{encode_id(member.id)}/roles",
            headers=headers,
        )
        assert user_roles_response.status_code == 200, user_roles_response.text
        assert [role["name"] for role in user_roles_response.json()["roles"]] == ["reports_viewer"]

        role_permissions_response = await client.get(
            f"/api/v1/roles/{role_id}/permissions",
            headers=headers,
        )
        assert role_permissions_response.status_code == 200, role_permissions_response.text
        assert role_permissions_response.json()["permissions"][0]["resource"] == "reports"
        assert role_permissions_response.json()["permissions"][0]["action"] == "read"

        check_permission_response = await client.get(
            f"/api/v1/check-permission/{encode_id(member.id)}",
            headers=headers,
            params={"resource": "reports", "action": "read"},
        )
        assert check_permission_response.status_code == 200, check_permission_response.text
        assert check_permission_response.json()["allowed"] is True

        casbin_roles_response = await client.get(
            f"/api/v1/casbin/roles/{encode_id(member.id)}",
            headers=headers,
        )
        assert casbin_roles_response.status_code == 200, casbin_roles_response.text
        assert casbin_roles_response.json()["roles"] == ["reports_viewer"]