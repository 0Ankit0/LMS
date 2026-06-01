from sqlalchemy import and_, desc, func, or_, select, update
from sqlalchemy.orm import Session


def col(attribute):
    return attribute


__all__ = [
    "Session",
    "and_",
    "col",
    "desc",
    "func",
    "or_",
    "select",
    "update",
]
