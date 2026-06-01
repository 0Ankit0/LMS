from datetime import datetime, timedelta

import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core.security import create_access_token, get_password_hash
from src.apps.iam.models.user import User
from src.apps.lms.models import (
    Assessment,
    AssessmentQuestion,
    Category,
    Course,
    Enrollment,
    ForumTopic,
    LmsNotification,
)


def _auth_header(user_id: int) -> dict[str, str]:
    token = create_access_token(str(user_id))
    return {"Authorization": f"Bearer {token}"}


async def _create_user(
    db_session: AsyncSession,
    username: str,
    email: str,
    password: str,
    *,
    is_superuser: bool = False,
) -> User:
    user = User(
        username=username,
        email=email,
        hashed_password=get_password_hash(password),
        is_superuser=is_superuser,
    )
    db_session.add(user)
    await db_session.commit()
    await db_session.refresh(user)
    return user


@pytest.mark.asyncio
async def test_auth_and_core_course_flow(client: AsyncClient, db_session: AsyncSession) -> None:
    admin = await _create_user(db_session, "admin", "admin@test.com", "StrongPass1!", is_superuser=True)
    student = await _create_user(db_session, "student", "student@test.com", "StrongPass1!")

    course_resp = await client.post(
        "/api/courses",
        json={
            "title": "Math 101",
            "description": "Intro course",
            "category": "Science",
            "level": "Beginner",
            "instructor_id": admin.id,
        },
        headers=_auth_header(admin.id),
    )
    assert course_resp.status_code == 201
    course_id = course_resp.json()["id"]

    module_resp = await client.post(
        f"/api/courses/{course_id}/modules",
        json={"title": "Module A", "description": "Basics", "order_index": 1},
        headers=_auth_header(admin.id),
    )
    assert module_resp.status_code == 201
    module_id = module_resp.json()["id"]

    lesson_resp = await client.post(
        f"/api/modules/{module_id}/lessons",
        json={"title": "Lesson 1", "content": "Content", "duration_minutes": 15},
        headers=_auth_header(admin.id),
    )
    assert lesson_resp.status_code == 201
    lesson_id = lesson_resp.json()["id"]

    enroll_resp = await client.post(f"/api/courses/{course_id}/enroll", headers=_auth_header(student.id))
    assert enroll_resp.status_code == 200

    progress_resp = await client.post(
        f"/api/lessons/{lesson_id}/progress",
        json={"timeSpent": "00:10:00", "isCompleted": True},
        headers=_auth_header(student.id),
    )
    assert progress_resp.status_code == 200
    assert "unlockedAchievements" in progress_resp.json()

    assessment = Assessment(course_id=course_id, title="Quiz 1", description="First quiz")
    db_session.add(assessment)
    await db_session.flush()
    question = AssessmentQuestion(
        assessment_id=assessment.id,
        prompt="2+2",
        options_json=["3", "4"],
        correct_answer="4",
        points=5,
        is_manual=False,
    )
    db_session.add(question)
    await db_session.commit()

    submit_resp = await client.post(
        f"/api/assessments/{assessment.id}/submit",
        json={"responses": {str(question.id): "4"}},
        headers=_auth_header(student.id),
    )
    assert submit_resp.status_code == 200
    attempt_id = submit_resp.json()["id"]

    grade_resp = await client.post(
        f"/api/assessment-attempts/{attempt_id}/grade",
        json={"manualScores": {"essay": 2}},
        headers=_auth_header(admin.id),
    )
    assert grade_resp.status_code == 200
    assert grade_resp.json()["status"] == "graded"

    gradebook_resp = await client.get(f"/api/courses/{course_id}/gradebook", headers=_auth_header(admin.id))
    assert gradebook_resp.status_code == 200
    assert len(gradebook_resp.json()) >= 1


@pytest.mark.asyncio
async def test_file_management_and_notes_conflict_flow(client: AsyncClient, db_session: AsyncSession) -> None:
    user = await _create_user(db_session, "notesuser", "notes@test.com", "StrongPass1!")
    headers = _auth_header(user.id)

    upload_resp = await client.post(
        "/api/my-files",
        headers=headers,
        files={"file": ("study.txt", b"hello", "text/plain")},
    )
    assert upload_resp.status_code == 201
    file_id = upload_resp.json()["id"]

    create_note_resp = await client.post(
        "/api/notes",
        headers=headers,
        json={
            "title": "My note",
            "content": "Important",
            "tag": "math",
            "priority": "high",
            "isPinned": True,
            "attachmentFileId": file_id,
        },
    )
    assert create_note_resp.status_code == 201
    note_id = create_note_resp.json()["id"]

    list_notes_resp = await client.get("/api/notes?tag=math&isPinned=true", headers=headers)
    assert list_notes_resp.status_code == 200
    assert any(item["id"] == note_id for item in list_notes_resp.json())

    delete_conflict_resp = await client.delete(f"/api/my-files/{file_id}", headers=headers)
    assert delete_conflict_resp.status_code == 409

    batch_delete_resp = await client.post(
        "/api/my-files/delete-batch",
        headers=headers,
        json={"fileIds": [file_id], "onConflict": "setNull"},
    )
    assert batch_delete_resp.status_code == 200
    assert file_id in batch_delete_resp.json()["deleted"]

    delete_note_resp = await client.delete(f"/api/notes/{note_id}", headers=headers)
    assert delete_note_resp.status_code == 204


@pytest.mark.asyncio
async def test_notifications_calendar_dropdowns_and_forum(client: AsyncClient, db_session: AsyncSession) -> None:
    admin = await _create_user(db_session, "opsadmin", "opsadmin@test.com", "StrongPass1!", is_superuser=True)
    student = await _create_user(db_session, "opsstudent", "opsstudent@test.com", "StrongPass1!")

    course = Course(
        title="History",
        description="Course",
        category="Arts",
        level="Intermediate",
        instructor_id=admin.id,
    )
    db_session.add(course)
    await db_session.flush()
    db_session.add(Enrollment(course_id=course.id, user_id=student.id))
    topic = ForumTopic(course_id=course.id, title="Introductions", created_by=admin.id)
    db_session.add(topic)
    db_session.add(Category(name="Arts"))
    db_session.add(LmsNotification(user_id=student.id, title="Welcome", message="Hello"))
    await db_session.commit()
    await db_session.refresh(topic)

    cal_create = await client.post(
        "/api/calendar-events",
        headers=_auth_header(admin.id),
        json={
            "courseId": course.id,
            "title": "Live class",
            "description": "Session",
            "startsAt": datetime.utcnow().isoformat(),
            "endsAt": (datetime.utcnow() + timedelta(hours=1)).isoformat(),
        },
    )
    assert cal_create.status_code == 201

    cal_resp = await client.get(
        "/api/calendar",
        headers=_auth_header(student.id),
        params={
            "startDate": (datetime.utcnow() - timedelta(days=1)).isoformat(),
            "endDate": (datetime.utcnow() + timedelta(days=1)).isoformat(),
        },
    )
    assert cal_resp.status_code == 200
    assert len(cal_resp.json()) >= 1

    notifications_resp = await client.get("/api/notifications", headers=_auth_header(student.id))
    assert notifications_resp.status_code == 200
    notification_id = notifications_resp.json()[0]["id"]

    mark_read_resp = await client.post(
        f"/api/notifications/{notification_id}/mark-as-read",
        headers=_auth_header(student.id),
    )
    assert mark_read_resp.status_code == 200
    assert mark_read_resp.json()["is_read"] is True

    dropdown_categories_resp = await client.get("/api/dropdown/categories")
    assert dropdown_categories_resp.status_code == 200
    assert len(dropdown_categories_resp.json()) >= 1

    dropdown_levels_resp = await client.get("/api/dropdown/course-levels")
    assert dropdown_levels_resp.status_code == 200
    assert len(dropdown_levels_resp.json()) >= 4

    dropdown_users_resp = await client.get("/api/dropdown/users", headers=_auth_header(admin.id))
    assert dropdown_users_resp.status_code == 200
    assert len(dropdown_users_resp.json()) >= 2

    forum_resp = await client.post(
        f"/api/forum-topics/{topic.id}/posts",
        headers=_auth_header(student.id),
        json={"content": "Glad to be here"},
    )
    assert forum_resp.status_code == 201
