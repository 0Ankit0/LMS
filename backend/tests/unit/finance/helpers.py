import base64
import hashlib
import hmac
import json


def esewa_sig(message: str, secret: str = "8gBm/:&EnhH.1/q") -> str:
    sig = hmac.new(secret.encode(), message.encode(), hashlib.sha256).digest()
    return base64.b64encode(sig).decode()


def esewa_signed_message(
    signed_field_names: str,
    payload: dict[str, str],
    *,
    include_field_names: bool = True,
) -> str:
    fields = [field.strip() for field in signed_field_names.split(",") if field.strip()]
    if include_field_names:
        return ",".join(f"{field}={payload[field]}" for field in fields)
    return ",".join(payload[field] for field in fields)


def esewa_callback_data(transaction_uuid: str, total_amount: int = 100) -> str:
    product_code = "EPAYTEST"
    signed_field_names = "transaction_code,status,total_amount,transaction_uuid,product_code,signed_field_names"
    fields_values = {
        "transaction_code": "TXNCODE123",
        "status": "COMPLETE",
        "total_amount": str(total_amount),
        "transaction_uuid": transaction_uuid,
        "product_code": product_code,
        "signed_field_names": signed_field_names,
    }
    message = esewa_signed_message(signed_field_names, fields_values)
    fields_values["signature"] = esewa_sig(message)
    return base64.b64encode(json.dumps(fields_values).encode()).decode()
