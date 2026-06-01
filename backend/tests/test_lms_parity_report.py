from __future__ import annotations

import json
from dataclasses import dataclass
from datetime import datetime, timedelta
from pathlib import Path

import pytest
from httpx import AsyncClient
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core.security import create_access_token, get_password_hash
from src.apps.iam.models.user import User
from src.apps.lms.models import (
    Achievement,
    Assessment,
    AssessmentQuestion,
    AttendanceRecord,
    Certificate,
    Course,
    Enrollment,
    ForumTopic,
    LmsNotification,
    UserAchievement,
)


@dataclass
class CallResult:
    method: str
    path: str
    status: int
    expected: str
    evidence: str


def _auth_header(user_id: int) -> dict[str, str]:
    return {"Authorization": f"Bearer {create_access_token(str(user_id))}"}


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


def _trim_evidence(payload: object) -> str:
    try:
        rendered = json.dumps(payload, default=str)
    except Exception:
        rendered = str(payload)
    if len(rendered) > 120:
        return rendered[:117] + "..."
    return rendered


async def _request_and_record(
    client: AsyncClient,
    results: list[CallResult],
    *,
    method: str,
    path: str,
    expected: set[int],
    headers: dict[str, str] | None = None,
    json_body: dict | None = None,
    params: dict | None = None,
    files: dict | None = None,
) -> dict | list | str | None:
    response = await client.request(
        method,
        path,
        headers=headers,
        json=json_body,
        params=params,
        files=files,
    )
    assert response.status_code in expected, f"{method} {path} -> {response.status_code}, expected {expected}, body={response.text}"

    payload: dict | list | str | None
    if response.status_code == 204:
        payload = None
        evidence = "no-content"
    else:
        try:
            payload = response.json()
        except Exception:
            payload = response.text
        evidence = _trim_evidence(payload)

    results.append(
        CallResult(
            method=method,
            path=path,
            status=response.status_code,
            expected="/".join(str(code) for code in sorted(expected)),
            evidence=evidence,
        )
    )
    return payload


@pytest.mark.asyncio
@pytest.mark.integration
async def test_generate_lms_endpoint_parity_report(client: AsyncClient, db_session: AsyncSession) -> None:
    results: list[CallResult] = []

    admin = await _create_user(db_session, "parity_admin", "parity_admin@test.com", "StrongPass1!", is_superuser=True)
    student = await _create_user(db_session, "parity_student", "parity_student@test.com", "StrongPass1!")
    parent = await _create_user(db_session, "parity_parent", "parity_parent@test.com", "StrongPass1!")

    admin_h = _auth_header(admin.id)
    student_h = _auth_header(student.id)
    parent_h = _auth_header(parent.id)

    # Authentication
    await _request_and_record(
        client,
        results,
        method="POST",
        path="/register",
        expected={200},
        json_body={"username": "reg_user", "email": "reg_user@test.com", "password": "StrongPass1!"},
    )
    login_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/login",
        expected={200},
        json_body={"email": "reg_user@test.com", "password": "StrongPass1!"},
    )
    reg_token = login_payload["access_token"]
    reg_h = {"Authorization": f"Bearer {reg_token}"}
    await _request_and_record(
        client,
        results,
        method="POST",
        path="/manage/info",
        expected={200},
        headers=reg_h,
        json_body={"first_name": "Reg", "last_name": "User", "phone": "123", "bio": "bio"},
    )
    await _request_and_record(client, results, method="POST", path="/logout", expected={200})

    # Courses, modules, lessons
    course_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/courses",
        expected={201},
        headers=admin_h,
        json_body={
            "title": "Parity Course",
            "description": "Course for parity",
            "category": "Science",
            "level": "Intermediate",
            "instructor_id": admin.id,
        },
    )
    course_id = course_payload["id"]

    await _request_and_record(
        client,
        results,
        method="GET",
        path="/api/courses",
        expected={200},
        params={"page": 1, "pageSize": 10, "search": "Parity", "category": "Science", "level": "Intermediate"},
    )
    await _request_and_record(client, results, method="GET", path=f"/api/courses/{course_id}", expected={200})

    module_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/courses/{course_id}/modules",
        expected={201},
        headers=admin_h,
        json_body={"title": "Parity Module", "description": "mod", "order_index": 1},
    )
    module_id = module_payload["id"]

    await _request_and_record(client, results, method="GET", path=f"/api/courses/{course_id}/modules", expected={200})
    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/modules/{module_id}",
        expected={204},
        headers=admin_h,
        json_body={"title": "Parity Module Updated", "description": "mod2", "order_index": 2},
    )

    lesson_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/modules/{module_id}/lessons",
        expected={201},
        headers=admin_h,
        json_body={"title": "Parity Lesson", "content": "lesson", "duration_minutes": 20},
    )
    lesson_id = lesson_payload["id"]

    await _request_and_record(client, results, method="GET", path=f"/api/modules/{module_id}/lessons", expected={200})
    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/lessons/{lesson_id}",
        expected={204},
        headers=admin_h,
        json_body={"title": "Parity Lesson Updated", "content": "lesson2", "duration_minutes": 25},
    )

    # Structure and progress
    await _request_and_record(client, results, method="GET", path=f"/api/courses/{course_id}/structure", expected={200}, headers=student_h)
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/lessons/{lesson_id}/progress",
        expected={200},
        headers=student_h,
        json_body={"timeSpent": "00:15:30", "isCompleted": True},
    )

    # Enrollments
    enroll_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/courses/{course_id}/enrollments",
        expected={201},
        headers=admin_h,
        json_body={"userId": student.id},
    )
    enrollment_id = enroll_payload["id"]
    await _request_and_record(client, results, method="GET", path=f"/api/courses/{course_id}/enrollments", expected={200}, headers=admin_h)
    await _request_and_record(client, results, method="POST", path=f"/api/courses/{course_id}/enroll", expected={200}, headers=student_h)
    await _request_and_record(client, results, method="GET", path="/api/my-enrollments", expected={200}, headers=student_h)

    # Assessments and gradebook
    assessment = Assessment(course_id=course_id, title="Parity Quiz", description="quiz")
    db_session.add(assessment)
    await db_session.flush()
    question = AssessmentQuestion(
        assessment_id=assessment.id,
        prompt="1+1",
        options_json=["1", "2"],
        correct_answer="2",
        points=5,
        is_manual=False,
    )
    db_session.add(question)
    await db_session.commit()

    await _request_and_record(client, results, method="GET", path=f"/api/assessments/{assessment.id}", expected={200})
    attempt_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/assessments/{assessment.id}/submit",
        expected={200},
        headers=student_h,
        json_body={"responses": {str(question.id): "2"}},
    )
    attempt_id = attempt_payload["id"]

    await _request_and_record(client, results, method="GET", path=f"/api/courses/{course_id}/gradebook", expected={200}, headers=admin_h)
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/assessment-attempts/{attempt_id}/grade",
        expected={200},
        headers=admin_h,
        json_body={"manualScores": {"essay": 1}},
    )

    # Announcements
    await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/announcements",
        expected={201},
        headers=admin_h,
        json_body={"title": "Parity Announcement", "content": "hello", "courseId": course_id},
    )
    await _request_and_record(client, results, method="GET", path="/api/announcements", expected={200}, params={"courseId": course_id})

    # Certificates and user management
    cert = Certificate(course_id=course_id, user_id=student.id, certificate_code="PARITY-CERT-001")
    db_session.add(cert)
    await db_session.commit()
    await db_session.refresh(cert)

    await _request_and_record(client, results, method="GET", path="/api/certificates", expected={200}, headers=admin_h)
    await _request_and_record(client, results, method="GET", path=f"/api/certificates/{cert.id}", expected={200}, headers=student_h)
    await _request_and_record(client, results, method="GET", path="/api/users/me/certificates", expected={200}, headers=student_h)

    await _request_and_record(client, results, method="GET", path="/api/admin/users", expected={200}, headers=admin_h, params={"page": 1, "pageSize": 20})
    await _request_and_record(client, results, method="GET", path=f"/api/admin/users/{student.id}", expected={200}, headers=admin_h)
    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/admin/users/{student.id}",
        expected={204},
        headers=admin_h,
        json_body={"is_active": True},
    )

    temp_user = await _create_user(db_session, "temp_delete", "temp_delete@test.com", "StrongPass1!")
    await _request_and_record(client, results, method="DELETE", path=f"/api/admin/users/{temp_user.id}", expected={204}, headers=admin_h)

    # Parent portal
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/admin/students/{student.id}/parent-links",
        expected={204},
        headers=admin_h,
        json_body={"parentId": parent.id},
    )
    await _request_and_record(client, results, method="GET", path="/api/parent/my-children", expected={200}, headers=parent_h)
    await _request_and_record(client, results, method="GET", path=f"/api/parent/children/{student.id}/dashboard", expected={200}, headers=parent_h)
    await _request_and_record(client, results, method="GET", path=f"/api/parent/children/{student.id}/grades", expected={200}, headers=parent_h)
    await _request_and_record(client, results, method="GET", path=f"/api/parent/children/{student.id}/attendance", expected={200}, headers=parent_h)

    # Admissions
    app_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/admissions/apply",
        expected={201},
        headers=student_h,
        json_body={"programId": "P-01", "payload": {"statement": "I want to learn"}},
    )
    application_id = app_payload["id"]

    await _request_and_record(client, results, method="GET", path="/api/admissions/my-application", expected={200}, headers=student_h)
    await _request_and_record(
        client,
        results,
        method="GET",
        path="/api/admin/admissions/applications",
        expected={200},
        headers=admin_h,
        params={"status": "Pending", "programId": "P-01", "page": 1, "pageSize": 10},
    )
    await _request_and_record(client, results, method="GET", path=f"/api/admin/admissions/applications/{application_id}", expected={200}, headers=admin_h)
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/admin/admissions/applications/{application_id}/decision",
        expected={204},
        headers=admin_h,
        json_body={"status": "Accepted", "reviewerNotes": "Looks good"},
    )

    # Competencies
    competency_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/competencies",
        expected={201},
        headers=admin_h,
        json_body={"name": "Critical Thinking", "description": "Ability to reason"},
    )
    competency_id = competency_payload["id"]

    await _request_and_record(client, results, method="GET", path="/api/competencies", expected={200}, headers=admin_h)
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/courses/{course_id}/competencies",
        expected={204},
        headers=admin_h,
        json_body={"competencyId": competency_id},
    )
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/assessments/{assessment.id}/competencies",
        expected={204},
        headers=admin_h,
        json_body={"competencyId": competency_id},
    )
    await _request_and_record(client, results, method="GET", path=f"/api/users/{student.id}/competency-profile", expected={200}, headers=admin_h)

    # Reports
    await _request_and_record(
        client,
        results,
        method="GET",
        path="/api/reports/student-engagement",
        expected={200},
        headers=admin_h,
        params={"courseId": course_id, "startDate": "2026-01-01", "endDate": "2026-12-31"},
    )
    await _request_and_record(
        client,
        results,
        method="GET",
        path="/api/reports/course-completion",
        expected={200},
        headers=admin_h,
        params={"courseId": course_id, "startDate": "2026-01-01", "endDate": "2026-12-31"},
    )

    # Attendance
    attendance_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/attendance",
        expected={200},
        headers=admin_h,
        json_body={
            "courseId": course_id,
            "sessionDate": "2026-05-01",
            "items": [{"userId": student.id, "status": "present", "notes": "ok"}],
        },
    )
    assert attendance_payload["created"] >= 1

    rows = (await db_session.execute(
        AttendanceRecord.__table__.select().where(AttendanceRecord.course_id == course_id)
    )).all()
    attendance_id = rows[0].id

    await _request_and_record(client, results, method="GET", path=f"/api/courses/{course_id}/attendance", expected={200}, headers=admin_h)
    await _request_and_record(client, results, method="GET", path=f"/api/users/{student.id}/attendance", expected={200}, headers=student_h)
    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/attendance/{attendance_id}",
        expected={200},
        headers=admin_h,
        json_body={"userId": student.id, "status": "late", "notes": "arrived late"},
    )

    # Gamification
    achievement_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/achievements",
        expected={201},
        headers=admin_h,
        json_body={"name": "First Steps", "description": "Completed first lesson", "points": 10},
    )
    achievement_id = achievement_payload["id"]

    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/achievements/{achievement_id}/criteria",
        expected={200},
        headers=admin_h,
        json_body={"criterion": {"event": "lesson_completed", "count": 1}},
    )

    db_session.add(UserAchievement(user_id=student.id, achievement_id=achievement_id))
    await db_session.commit()

    await _request_and_record(client, results, method="GET", path="/api/leaderboard", expected={200})
    await _request_and_record(client, results, method="GET", path=f"/api/users/{student.id}/achievements", expected={200})

    # Categories and tags
    category_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/categories",
        expected={201},
        headers=admin_h,
        json_body={"name": "Programming", "parentId": None},
    )
    category_id = category_payload["id"]

    await _request_and_record(client, results, method="GET", path="/api/categories", expected={200})
    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/categories/{category_id}",
        expected={200},
        headers=admin_h,
        json_body={"name": "Programming Updated", "parentId": None},
    )

    tag_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/tags",
        expected={201},
        headers=admin_h,
        json_body={"name": "python"},
    )
    tag_id = tag_payload["id"]

    await _request_and_record(client, results, method="GET", path="/api/tags", expected={200})
    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/tags/{tag_id}",
        expected={200},
        headers=admin_h,
        json_body={"name": "python-advanced"},
    )

    # Communication
    conversation_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/conversations",
        expected={201},
        headers=student_h,
        json_body={"participantIds": [admin.id], "title": "Direct"},
    )
    conversation_id = conversation_payload["id"]

    await _request_and_record(client, results, method="GET", path="/api/conversations", expected={200}, headers=student_h)
    await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/messages",
        expected={201},
        headers=student_h,
        json_body={"conversationId": conversation_id, "encryptedContent": "ciphertext"},
    )
    await _request_and_record(client, results, method="GET", path=f"/api/conversations/{conversation_id}/messages", expected={200}, headers=student_h)

    topic = ForumTopic(course_id=course_id, title="Parity Forum", created_by=admin.id)
    db_session.add(topic)
    await db_session.commit()
    await db_session.refresh(topic)
    await _request_and_record(
        client,
        results,
        method="POST",
        path=f"/api/forum-topics/{topic.id}/posts",
        expected={201},
        headers=student_h,
        json_body={"content": "Forum post"},
    )

    # File management, notifications, calendar, dropdowns, notes
    upload_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/my-files",
        expected={201},
        headers=student_h,
        files={"file": ("parity.txt", b"content", "text/plain")},
    )
    file_id = upload_payload["id"]

    await _request_and_record(client, results, method="GET", path="/api/my-files", expected={200}, headers=student_h)

    note_payload = await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/notes",
        expected={201},
        headers=student_h,
        json_body={
            "title": "Parity Note",
            "content": "Remember this",
            "courseId": course_id,
            "lessonId": lesson_id,
            "tag": "study",
            "priority": "high",
            "isPinned": True,
            "attachmentFileId": file_id,
        },
    )
    note_id = note_payload["id"]

    await _request_and_record(
        client,
        results,
        method="GET",
        path="/api/notes",
        expected={200},
        headers=student_h,
        params={"courseId": course_id, "lessonId": lesson_id, "searchTerm": "Remember", "tag": "study", "priority": "high", "isPinned": True},
    )
    await _request_and_record(client, results, method="GET", path=f"/api/lessons/{lesson_id}/notes", expected={200}, headers=student_h)

    await _request_and_record(
        client,
        results,
        method="PUT",
        path=f"/api/notes/{note_id}",
        expected={200},
        headers=student_h,
        json_body={"title": "Parity Note Updated", "isPinned": False},
    )

    await _request_and_record(client, results, method="DELETE", path=f"/api/my-files/{file_id}", expected={409}, headers=student_h)
    await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/my-files/delete-batch",
        expected={200},
        headers=student_h,
        json_body={"fileIds": [file_id], "onConflict": "setNull"},
    )
    await _request_and_record(client, results, method="DELETE", path=f"/api/notes/{note_id}", expected={204}, headers=student_h)

    db_session.add(LmsNotification(user_id=student.id, title="Parity Notification", message="Test"))
    await db_session.commit()

    notification_list = await _request_and_record(client, results, method="GET", path="/api/notifications", expected={200}, headers=student_h)
    notification_id = notification_list[0]["id"]
    await _request_and_record(client, results, method="POST", path=f"/api/notifications/{notification_id}/mark-as-read", expected={200}, headers=student_h)

    starts_at = datetime.utcnow()
    ends_at = starts_at + timedelta(hours=1)
    await _request_and_record(
        client,
        results,
        method="POST",
        path="/api/calendar-events",
        expected={201},
        headers=admin_h,
        json_body={
            "courseId": course_id,
            "title": "Parity Event",
            "description": "Calendar",
            "startsAt": starts_at.isoformat(),
            "endsAt": ends_at.isoformat(),
        },
    )
    await _request_and_record(
        client,
        results,
        method="GET",
        path="/api/calendar",
        expected={200},
        headers=student_h,
        params={"startDate": (starts_at - timedelta(days=1)).isoformat(), "endDate": (ends_at + timedelta(days=1)).isoformat()},
    )

    await _request_and_record(client, results, method="GET", path="/api/dropdown/categories", expected={200})
    await _request_and_record(client, results, method="GET", path="/api/dropdown/course-levels", expected={200})
    await _request_and_record(client, results, method="GET", path="/api/dropdown/users", expected={200}, headers=admin_h)

    # Cleanup deletion endpoints placed at end so primary resources stay available for previous checks.
    await _request_and_record(client, results, method="DELETE", path=f"/api/enrollments/{enrollment_id}", expected={204}, headers=admin_h)
    await _request_and_record(client, results, method="DELETE", path=f"/api/lessons/{lesson_id}", expected={204}, headers=admin_h)
    await _request_and_record(client, results, method="DELETE", path=f"/api/modules/{module_id}", expected={204}, headers=admin_h)
    await _request_and_record(client, results, method="DELETE", path=f"/api/tags/{tag_id}", expected={204}, headers=admin_h)
    await _request_and_record(client, results, method="DELETE", path=f"/api/categories/{category_id}", expected={204}, headers=admin_h)
    await _request_and_record(client, results, method="DELETE", path=f"/api/admin/students/{student.id}/parent-links/{parent.id}", expected={204}, headers=admin_h)
    await _request_and_record(client, results, method="DELETE", path=f"/api/courses/{course_id}", expected={204}, headers=admin_h)

    # Generate parity report document from live calls.
    report_lines = [
        "# 20. Endpoint Parity Report (Live Validation)",
        "",
        f"Generated: {datetime.utcnow().isoformat()}Z",
        "",
        "This report is generated from live API calls executed in integration tests.",
        "",
        "## Summary",
        "",
        f"- Total endpoint validations: {len(results)}",
        f"- Successful validations: {len(results)}",
        "- Failed validations: 0",
        "",
        "## Live Endpoint Matrix",
        "",
        "| Method | Endpoint | Expected | Actual | Evidence |",
        "|---|---|---|---|---|",
    ]

    for item in results:
        evidence = item.evidence.replace("|", "\\|")
        report_lines.append(f"| {item.method} | {item.path} | {item.expected} | {item.status} | {evidence} |")

    report_text = "\n".join(report_lines) + "\n"
    report_path = Path(__file__).resolve().parents[2] / "docs" / "requirements" / "lms" / "20-Endpoint-Parity-Report.md"
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(report_text, encoding="utf-8")

    assert report_path.exists()
