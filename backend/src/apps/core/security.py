from datetime import datetime, timedelta, timezone
from typing import Any, Union
from enum import Enum
import uuid
import base64
import hashlib
import json

from passlib.context import CryptContext
import pyseto
from pyseto import Key, PysetoError

from src.apps.core.config import settings

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")


def _apply_pepper(password: str) -> str:
    """Append a secret pepper to the password before hashing/verifying.

    The pepper is an additional secret value stored in the application
    configuration layer, so if an attacker obtains the password hashes they
    still need the pepper to brute-force passwords.
    """
    pepper = settings.PASSWORD_PEPPER or ""
    return password + pepper

ALGORITHM = "v4.local"


class TokenValidationError(ValueError):
    pass


class TokenType(str, Enum):
    ACCESS = "access"
    REFRESH = "refresh"
    PASSWORD_RESET = "password_reset"
    EMAIL_VERIFICATION = "email_verification"
    TEMP_AUTH = "temp_auth"
    BEARER = "bearer"


def _get_paseto_key() -> Key:
    secret_material = settings.PASETO_SECRET_KEY or settings.SECRET_KEY
    derived_key = hashlib.blake2b(secret_material.encode(), digest_size=32).digest()
    return Key.new(version=4, purpose="local", key=derived_key)


def _encode_payload(payload: dict[str, Any]) -> str:
    token = pyseto.encode(_get_paseto_key(), payload, serializer=json)
    if isinstance(token, bytes):
        return token.decode()
    return str(token)


def _coerce_expiration(payload: dict[str, Any]) -> datetime:
    raw_exp = payload.get("exp")
    if isinstance(raw_exp, datetime):
        return raw_exp if raw_exp.tzinfo else raw_exp.replace(tzinfo=timezone.utc)
    if isinstance(raw_exp, int | float):
        return datetime.fromtimestamp(raw_exp, tz=timezone.utc)
    if isinstance(raw_exp, str):
        if raw_exp.isdigit():
            return datetime.fromtimestamp(int(raw_exp), tz=timezone.utc)
        try:
            normalized = raw_exp.replace("Z", "+00:00")
            parsed = datetime.fromisoformat(normalized)
        except ValueError as exc:
            raise TokenValidationError("Token is missing a valid expiration") from exc
        return parsed if parsed.tzinfo else parsed.replace(tzinfo=timezone.utc)
    raise TokenValidationError("Token is missing a valid expiration")


def _build_token_payload(
    subject: Union[str, Any],
    token_type: str,
    expires_at: datetime,
    extra_claims: dict[str, Any] | None = None,
) -> dict[str, Any]:
    payload: dict[str, Any] = {
        "exp": expires_at.astimezone(timezone.utc).isoformat(),
        "sub": str(subject),
        "type": token_type,
        "jti": str(uuid.uuid4()),
    }
    if extra_claims:
        payload.update(extra_claims)
    return payload


def decode_token(token: str) -> dict[str, Any]:
    try:
        decoded = pyseto.decode(_get_paseto_key(), token, deserializer=json)
    except Exception as exc:
        raise TokenValidationError("Invalid token") from exc

    payload = decoded.payload
    if not isinstance(payload, dict):
        raise TokenValidationError("Invalid token payload")

    exp = _coerce_expiration(payload)
    if exp <= datetime.now(timezone.utc):
        raise TokenValidationError("Token has expired")

    return payload


def payload_expiration(payload: dict[str, Any]) -> datetime:
    return _coerce_expiration(payload)


def create_access_token(
    subject: Union[str, Any],
    expires_delta: timedelta | None = None,
) -> str:
    expire = (
        datetime.now(timezone.utc) + expires_delta
        if expires_delta
        else datetime.now(timezone.utc) + timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
    )
    return _encode_payload(_build_token_payload(subject, TokenType.ACCESS.value, expire))

def create_refresh_token(subject: Union[str, Any], expires_delta: timedelta | None = None) -> str:
    expire = (
        datetime.now(timezone.utc) + expires_delta
        if expires_delta
        else datetime.now(timezone.utc) + timedelta(days=settings.REFRESH_TOKEN_EXPIRE_DAYS)
    )
    return _encode_payload(_build_token_payload(subject, TokenType.REFRESH.value, expire))

def create_password_reset_token(subject: Union[str, Any]) -> str:
    """Create a password reset token valid for 1 hour"""
    expire = datetime.now(timezone.utc) + timedelta(hours=1)
    return _encode_payload(_build_token_payload(subject, TokenType.PASSWORD_RESET.value, expire))

def create_email_verification_token(subject: Union[str, Any]) -> str:
    """Create an email verification token valid for 24 hours"""
    expire = datetime.now(timezone.utc) + timedelta(hours=24)
    return _encode_payload(_build_token_payload(subject, TokenType.EMAIL_VERIFICATION.value, expire))

def create_temp_auth_token(subject: Union[str, Any]) -> str:
    """Create a temporary auth token for OTP validation, valid for 5 minutes"""
    expire = datetime.now(timezone.utc) + timedelta(minutes=5)
    return _encode_payload(_build_token_payload(subject, TokenType.TEMP_AUTH.value, expire))

def verify_token(token: str, token_type: TokenType | None = None) -> dict:
    """
    Decode and verify a PASETO token.
    If token_type is provided, checks that the 'type' claim matches.
    Raises TokenValidationError if the token is invalid, expired, or has wrong type.
    Returns the payload dictionary on success.
    """
    payload = decode_token(token)
    if token_type and payload.get("type") != token_type.value:
        raise TokenValidationError(f"Invalid token type, expected {token_type.value}")
    return payload

def verify_password(plain_password: str, hashed_password: str) -> bool:
    return pwd_context.verify(_apply_pepper(plain_password), hashed_password)


def get_password_hash(password: str) -> str:
    return pwd_context.hash(_apply_pepper(password))


def validate_password_strength(password: str) -> None:
    """Validate password strength based on current settings.

    Raises ValueError when the password does not meet requirements.
    """
    if len(password) < settings.PASSWORD_MIN_LENGTH:
        raise ValueError(f"Password must be at least {settings.PASSWORD_MIN_LENGTH} characters long")

    if settings.PASSWORD_REQUIRE_UPPERCASE and not any(c.isupper() for c in password):
        raise ValueError("Password must contain at least one uppercase letter")

    if settings.PASSWORD_REQUIRE_LOWERCASE and not any(c.islower() for c in password):
        raise ValueError("Password must contain at least one lowercase letter")

    if settings.PASSWORD_REQUIRE_DIGIT and not any(c.isdigit() for c in password):
        raise ValueError("Password must contain at least one digit")

    if settings.PASSWORD_REQUIRE_SPECIAL:
        special_chars = "!@#$%^&*()-_=+[]{}|;:'\",.<>?/~`"
        if not any(c in special_chars for c in password):
            raise ValueError("Password must contain at least one special character")


def create_secure_url_token(data: dict[str, Any], expires_hours: int = 24) -> str:
    """
    Create a secure, encrypted, tamper-proof URL token.
    Data is encrypted and authenticated using PASETO.
    Returns a URL-safe base64 encoded token.
    """
    expire = datetime.now(timezone.utc) + timedelta(hours=expires_hours)
    encoded_token = _encode_payload(
        _build_token_payload(
            "secure-url",
            "secure_url",
            expire,
            extra_claims={"data": data},
        )
    )
    # Make it URL-safe
    url_safe_token = base64.urlsafe_b64encode(encoded_token.encode()).decode()
    return url_safe_token


def verify_secure_url_token(url_token: str) -> dict[str, Any]:
    """
    Verify and decrypt a secure URL token.
    Returns the data dict if valid, raises TokenValidationError if invalid/expired/tampered.
    """
    try:
        # Decode from URL-safe base64
        encoded_token = base64.urlsafe_b64decode(url_token.encode()).decode()
        payload = decode_token(encoded_token)
        if payload.get("type") != "secure_url":
            raise TokenValidationError("Invalid secure URL token type")
        return payload.get("data", {})
    except Exception as e:
        raise TokenValidationError(f"Invalid or tampered token: {str(e)}")


def create_oauth_state(provider: str) -> str:
    """Create a short-lived PASETO used as the OAuth2 *state* parameter for CSRF protection."""
    expire = datetime.now(timezone.utc) + timedelta(minutes=10)
    payload = _build_token_payload(
        provider,
        "oauth_state",
        expire,
        extra_claims={"nonce": str(uuid.uuid4())},
    )
    return _encode_payload(payload)


def verify_oauth_state(state: str, provider: str) -> bool:
    """Return True if *state* is a valid, unexpired oauth_state token for *provider*."""
    try:
        payload = decode_token(state)
        return payload.get("type") == "oauth_state" and payload.get("sub") == provider
    except TokenValidationError:
        return False
