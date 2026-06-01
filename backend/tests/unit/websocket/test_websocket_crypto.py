import base64
import pytest

from src.apps.websocket.crypto import decrypt, derive_session_key, encrypt, session_key_b64


class TestCrypto:
    @pytest.mark.unit
    def test_encrypt_decrypt_roundtrip(self):
        key = derive_session_key("test-jti-123")
        plaintext = b'{"type":"ping"}'
        iv_b64, ct_b64 = encrypt(plaintext, key)
        assert iv_b64 != ""
        assert ct_b64 != ""
        assert decrypt(iv_b64, ct_b64, key) == plaintext

    @pytest.mark.unit
    def test_different_keys_fail_decrypt(self):
        key1 = derive_session_key("jti-aaa")
        key2 = derive_session_key("jti-bbb")
        iv_b64, ct_b64 = encrypt(b"secret", key1)
        from cryptography.exceptions import InvalidTag

        with pytest.raises(InvalidTag):
            decrypt(iv_b64, ct_b64, key2)

    @pytest.mark.unit
    def test_session_key_deterministic(self):
        assert derive_session_key("fixed-jti") == derive_session_key("fixed-jti")

    @pytest.mark.unit
    def test_session_key_unique_per_jti(self):
        assert derive_session_key("jti-x") != derive_session_key("jti-y")

    @pytest.mark.unit
    def test_session_key_b64_is_32_bytes(self):
        raw = base64.b64decode(session_key_b64("any-jti"))
        assert len(raw) == 32

    @pytest.mark.unit
    def test_encrypt_nonce_is_unique(self):
        key = derive_session_key("jti-nonce-test")
        iv1, _ = encrypt(b"hello", key)
        iv2, _ = encrypt(b"hello", key)
        assert iv1 != iv2
