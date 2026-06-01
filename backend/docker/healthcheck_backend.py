import sys
import urllib.request


def main() -> int:
    with urllib.request.urlopen("http://127.0.0.1:8000/api/v1/system/health/", timeout=5) as response:
        return 0 if response.status == 200 else 1


if __name__ == "__main__":
    raise SystemExit(main())
