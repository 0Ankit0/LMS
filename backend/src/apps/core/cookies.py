from __future__ import annotations

from fastapi import Response

from src.apps.core.config import settings


def auth_cookie_options(*, max_age: int) -> dict[str, object]:
    options: dict[str, object] = {
        "httponly": True,
        "secure": settings.SECURE_COOKIES,
        "samesite": settings.COOKIE_SAMESITE,
        "max_age": max_age,
        "path": "/",
    }
    if settings.COOKIE_DOMAIN:
        options["domain"] = settings.COOKIE_DOMAIN
    return options


def set_auth_cookies(
    response: Response,
    *,
    access_token: str,
    refresh_token: str,
) -> None:
    response.set_cookie(
        key=settings.ACCESS_TOKEN_COOKIE,
        value=access_token,
        **auth_cookie_options(max_age=settings.ACCESS_TOKEN_EXPIRE_MINUTES * 60),
    )
    response.set_cookie(
        key=settings.REFRESH_TOKEN_COOKIE,
        value=refresh_token,
        **auth_cookie_options(max_age=settings.REFRESH_TOKEN_EXPIRE_DAYS * 24 * 60 * 60),
    )


def clear_auth_cookies(response: Response) -> None:
    delete_options = {
        "domain": settings.COOKIE_DOMAIN,
        "secure": settings.SECURE_COOKIES,
        "httponly": True,
        "samesite": settings.COOKIE_SAMESITE,
        "path": "/",
    }
    response.delete_cookie(key=settings.ACCESS_TOKEN_COOKIE, **delete_options)
    response.delete_cookie(key=settings.REFRESH_TOKEN_COOKIE, **delete_options)
