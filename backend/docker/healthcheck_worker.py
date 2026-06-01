import socket


def main() -> int:
    connection = socket.create_connection(("redis", 6379), 5)
    connection.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
