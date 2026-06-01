from __future__ import annotations

import asyncio
from datetime import date, datetime, timedelta
from pathlib import Path

from src.apps.core.security import get_password_hash
from src.apps.iam.models.user import User, UserProfile
from src.apps.lms.models import (
    Achievement,
    AdmissionApplication,
    Announcement,
    Assessment,
    AssessmentAttempt,
    AssessmentCompetency,
    AssessmentQuestion,
    AttendanceRecord,
    CalendarEvent,
    Category,
    Certificate,
    Competency,
    Conversation,
    ConversationParticipant,
    Course,
    CourseCompetency,
    CourseTag,
    Enrollment,
    ForumPost,
    ForumTopic,
    Lesson,
    LessonProgress,
    LmsNotification,
    MediaFile,
    Message,
    Module,
    Note,
    ParentStudentLink,
    Tag,
    UserAchievement,
    UserCompetency,
)
from sqlalchemy import or_

from src.db.query import select
from src.db.session import async_session_factory, init_db


QA_USERS = {
    "admin": {
        "username": "qa_admin",
        "email": "admin@lms-qa.example.com",
        "password": "Admin123!",
        "is_superuser": True,
        "first_name": "LMS",
        "last_name": "Admin",
    },
    "instructor": {
        "username": "qa_instructor",
        "email": "instructor@lms-qa.example.com",
        "password": "Instructor123!",
        "is_superuser": False,
        "first_name": "Course",
        "last_name": "Instructor",
    },
    "student": {
        "username": "qa_student",
        "email": "student@lms-qa.example.com",
        "password": "Student123!",
        "is_superuser": False,
        "first_name": "Primary",
        "last_name": "Student",
    },
    "student_two": {
        "username": "qa_student_two",
        "email": "student2@lms-qa.example.com",
        "password": "Student123!",
        "is_superuser": False,
        "first_name": "Secondary",
        "last_name": "Student",
    },
    "parent": {
        "username": "qa_parent",
        "email": "parent@lms-qa.example.com",
        "password": "Parent123!",
        "is_superuser": False,
        "first_name": "Supportive",
        "last_name": "Parent",
    },
}


def _now() -> datetime:
    return datetime.utcnow()


async def _get_or_create_user(session, *, username: str, email: str, password: str, is_superuser: bool, first_name: str, last_name: str) -> User:
    result = await session.execute(select(User).where(or_(User.email == email, User.username == username)))
    user = result.scalars().first()

    if user is None:
        user = User(
            username=username,
            email=email,
            hashed_password=get_password_hash(password),
            is_active=True,
            is_superuser=is_superuser,
            is_confirmed=True,
        )
        session.add(user)
        await session.flush()
    else:
        user.username = username
        user.email = email
        user.hashed_password = get_password_hash(password)
        user.is_active = True
        user.is_superuser = is_superuser
        user.is_confirmed = True

    profile_result = await session.execute(select(UserProfile).where(UserProfile.user_id == user.id))
    profile = profile_result.scalars().first()
    if profile is None:
        profile = UserProfile(user_id=user.id, first_name=first_name, last_name=last_name, phone="", bio="")
        session.add(profile)
    else:
        profile.first_name = first_name
        profile.last_name = last_name

    return user


async def _get_or_create_by_field(session, model, field_name: str, field_value, **defaults):
    field = getattr(model, field_name)
    result = await session.execute(select(model).where(field == field_value))
    row = result.scalars().first()
    if row is None:
        row = model(**{field_name: field_value, **defaults})
        session.add(row)
        await session.flush()
        return row

    for key, value in defaults.items():
        setattr(row, key, value)
    return row


async def seed() -> None:
    await init_db()

    async with async_session_factory() as session:
        users: dict[str, User] = {}
        for key, payload in QA_USERS.items():
            users[key] = await _get_or_create_user(session, **payload)

        category_stem = await _get_or_create_by_field(session, Category, "name", "STEM", parent_id=None)
        category_business = await _get_or_create_by_field(session, Category, "name", "Business", parent_id=None)

        tag_fastapi = await _get_or_create_by_field(session, Tag, "name", "fastapi")
        tag_python = await _get_or_create_by_field(session, Tag, "name", "python")

        course_fastapi = await _get_or_create_by_field(
            session,
            Course,
            "title",
            "Intro to FastAPI",
            description="Build APIs with FastAPI, async SQLAlchemy, and testing fundamentals.",
            category=category_stem.name,
            level="Beginner",
            instructor_id=users["instructor"].id,
            updated_at=_now(),
        )

        course_data = await _get_or_create_by_field(
            session,
            Course,
            "title",
            "Data Analysis Essentials",
            description="Core analytics concepts for LMS reporting and decision-making.",
            category=category_business.name,
            level="Intermediate",
            instructor_id=users["instructor"].id,
            updated_at=_now(),
        )

        module_one = await _get_or_create_by_field(
            session,
            Module,
            "title",
            "FastAPI Fundamentals",
            course_id=course_fastapi.id,
            description="Routing, request validation, and dependency injection.",
            order_index=1,
        )

        lesson_one = await _get_or_create_by_field(
            session,
            Lesson,
            "title",
            "Dependency Injection and Routing",
            module_id=module_one.id,
            content="FastAPI dependency injection patterns and route handlers.",
            duration_minutes=35,
        )

        lesson_two = await _get_or_create_by_field(
            session,
            Lesson,
            "title",
            "Async SQLAlchemy Sessions",
            module_id=module_one.id,
            content="Using async sessions safely in web request lifecycle.",
            duration_minutes=45,
        )

        await _get_or_create_by_field(
            session,
            Enrollment,
            "user_id",
            users["student"].id,
            course_id=course_fastapi.id,
            role="student",
            enrolled_at=_now(),
        )
        await _get_or_create_by_field(
            session,
            Enrollment,
            "user_id",
            users["student_two"].id,
            course_id=course_fastapi.id,
            role="student",
            enrolled_at=_now(),
        )

        result = await session.execute(
            select(Enrollment).where(
                Enrollment.user_id == users["instructor"].id,
                Enrollment.course_id == course_fastapi.id,
                Enrollment.role == "instructor",
            )
        )
        if result.scalars().first() is None:
            session.add(
                Enrollment(
                    user_id=users["instructor"].id,
                    course_id=course_fastapi.id,
                    role="instructor",
                )
            )

        await _get_or_create_by_field(
            session,
            Announcement,
            "title",
            "Welcome to FastAPI Course",
            content="Please complete module 1 and submit your first assessment this week.",
            course_id=course_fastapi.id,
            created_by=users["admin"].id,
            created_at=_now(),
        )

        assessment = await _get_or_create_by_field(
            session,
            Assessment,
            "title",
            "FastAPI Basics Quiz",
            course_id=course_fastapi.id,
            description="Short quiz covering first module concepts.",
            max_score=100,
        )

        question = await _get_or_create_by_field(
            session,
            AssessmentQuestion,
            "prompt",
            "Which object is used for dependency injection in FastAPI routes?",
            assessment_id=assessment.id,
            options_json=["Depends", "Include", "Router", "Middleware"],
            correct_answer="Depends",
            points=5,
            is_manual=False,
        )

        await _get_or_create_by_field(
            session,
            AssessmentAttempt,
            "user_id",
            users["student"].id,
            assessment_id=assessment.id,
            responses_json={str(question.id): "Depends"},
            score=5,
            status="graded",
            graded_by=users["instructor"].id,
            submitted_at=_now(),
        )

        await _get_or_create_by_field(
            session,
            Certificate,
            "certificate_code",
            "QA-FASTAPI-001",
            course_id=course_fastapi.id,
            user_id=users["student"].id,
            issued_at=_now(),
        )

        result = await session.execute(
            select(AttendanceRecord).where(
                AttendanceRecord.course_id == course_fastapi.id,
                AttendanceRecord.user_id == users["student"].id,
                AttendanceRecord.session_date == date.today(),
            )
        )
        if result.scalars().first() is None:
            session.add(
                AttendanceRecord(
                    course_id=course_fastapi.id,
                    user_id=users["student"].id,
                    session_date=date.today(),
                    status="present",
                    notes="On time",
                    marked_by=users["instructor"].id,
                )
            )

        achievement = await _get_or_create_by_field(
            session,
            Achievement,
            "name",
            "Fast Starter",
            description="Complete first module and submit first quiz.",
            points=50,
            criteria_json=[{"type": "module_completion", "threshold": 1}],
        )

        result = await session.execute(
            select(UserAchievement).where(
                UserAchievement.user_id == users["student"].id,
                UserAchievement.achievement_id == achievement.id,
            )
        )
        if result.scalars().first() is None:
            session.add(UserAchievement(user_id=users["student"].id, achievement_id=achievement.id))

        result = await session.execute(
            select(CourseTag).where(CourseTag.course_id == course_fastapi.id, CourseTag.tag_id == tag_fastapi.id)
        )
        if result.scalars().first() is None:
            session.add(CourseTag(course_id=course_fastapi.id, tag_id=tag_fastapi.id))

        result = await session.execute(
            select(CourseTag).where(CourseTag.course_id == course_data.id, CourseTag.tag_id == tag_python.id)
        )
        if result.scalars().first() is None:
            session.add(CourseTag(course_id=course_data.id, tag_id=tag_python.id))

        conversation = await _get_or_create_by_field(
            session,
            Conversation,
            "title",
            "FastAPI Course Support",
            is_group=True,
            created_by=users["instructor"].id,
            created_at=_now(),
        )

        for participant in (users["instructor"], users["student"], users["student_two"]):
            result = await session.execute(
                select(ConversationParticipant).where(
                    ConversationParticipant.conversation_id == conversation.id,
                    ConversationParticipant.user_id == participant.id,
                )
            )
            if result.scalars().first() is None:
                session.add(ConversationParticipant(conversation_id=conversation.id, user_id=participant.id))

        await _get_or_create_by_field(
            session,
            Message,
            "encrypted_content",
            "Welcome to the support channel. Ask your questions here.",
            conversation_id=conversation.id,
            sender_id=users["instructor"].id,
            created_at=_now(),
        )
        await _get_or_create_by_field(
            session,
            Message,
            "encrypted_content",
            "Thanks! I have completed lesson one and started lesson two.",
            conversation_id=conversation.id,
            sender_id=users["student"].id,
            created_at=_now(),
        )

        topic = await _get_or_create_by_field(
            session,
            ForumTopic,
            "title",
            "Best practices for async endpoint tests",
            course_id=course_fastapi.id,
            created_by=users["instructor"].id,
            created_at=_now(),
        )

        await _get_or_create_by_field(
            session,
            ForumPost,
            "content",
            "Use integration tests with seeded data and assert behavior, not implementation details.",
            topic_id=topic.id,
            user_id=users["instructor"].id,
            created_at=_now(),
        )

        result = await session.execute(
            select(ParentStudentLink).where(
                ParentStudentLink.parent_id == users["parent"].id,
                ParentStudentLink.student_id == users["student"].id,
            )
        )
        if result.scalars().first() is None:
            session.add(ParentStudentLink(parent_id=users["parent"].id, student_id=users["student"].id))

        result = await session.execute(
            select(AdmissionApplication).where(
                AdmissionApplication.user_id == users["student_two"].id,
                AdmissionApplication.program_id == "CS-2026",
            )
        )
        if result.scalars().first() is None:
            session.add(
                AdmissionApplication(
                    user_id=users["student_two"].id,
                    program_id="CS-2026",
                    status="Pending",
                    reviewer_notes="",
                    payload_json={"statement": "Interested in backend systems"},
                    submitted_at=_now(),
                    updated_at=_now(),
                )
            )

        competency = await _get_or_create_by_field(
            session,
            Competency,
            "name",
            "API Design Fundamentals",
            description="Designing robust and maintainable API interfaces.",
        )

        result = await session.execute(
            select(CourseCompetency).where(
                CourseCompetency.course_id == course_fastapi.id,
                CourseCompetency.competency_id == competency.id,
            )
        )
        if result.scalars().first() is None:
            session.add(CourseCompetency(course_id=course_fastapi.id, competency_id=competency.id))

        result = await session.execute(
            select(AssessmentCompetency).where(
                AssessmentCompetency.assessment_id == assessment.id,
                AssessmentCompetency.competency_id == competency.id,
            )
        )
        if result.scalars().first() is None:
            session.add(AssessmentCompetency(assessment_id=assessment.id, competency_id=competency.id))

        result = await session.execute(
            select(UserCompetency).where(
                UserCompetency.user_id == users["student"].id,
                UserCompetency.competency_id == competency.id,
            )
        )
        if result.scalars().first() is None:
            session.add(
                UserCompetency(
                    user_id=users["student"].id,
                    competency_id=competency.id,
                    level=2,
                    last_updated=_now(),
                )
            )

        seed_media_dir = Path("media/lms-seed")
        seed_media_dir.mkdir(parents=True, exist_ok=True)
        sample_file_path = seed_media_dir / "qa-course-guide.txt"
        if not sample_file_path.exists():
            sample_file_path.write_text("LMS QA seeded file for file-management page checks.\n", encoding="utf-8")

        media_row = await _get_or_create_by_field(
            session,
            MediaFile,
            "stored_name",
            "qa-course-guide.txt",
            user_id=users["student"].id,
            original_name="qa-course-guide.txt",
            content_type="text/plain",
            size_bytes=sample_file_path.stat().st_size,
            storage_path=str(sample_file_path),
            uploaded_at=_now(),
        )

        await _get_or_create_by_field(
            session,
            Note,
            "title",
            "Dependency Injection Summary",
            user_id=users["student"].id,
            course_id=course_fastapi.id,
            lesson_id=lesson_one.id,
            content="Depends helps inject database sessions and auth context safely.",
            tag="study",
            priority="high",
            is_pinned=True,
            attachment_file_id=media_row.id,
            created_at=_now(),
            updated_at=_now(),
        )

        result = await session.execute(
            select(LessonProgress).where(
                LessonProgress.lesson_id == lesson_one.id,
                LessonProgress.user_id == users["student"].id,
            )
        )
        progress = result.scalars().first()
        if progress is None:
            progress = LessonProgress(
                lesson_id=lesson_one.id,
                user_id=users["student"].id,
                time_spent_seconds=2100,
                is_completed=True,
                updated_at=_now(),
            )
            session.add(progress)
        else:
            progress.time_spent_seconds = 2100
            progress.is_completed = True
            progress.updated_at = _now()

        result = await session.execute(
            select(LessonProgress).where(
                LessonProgress.lesson_id == lesson_two.id,
                LessonProgress.user_id == users["student"].id,
            )
        )
        progress = result.scalars().first()
        if progress is None:
            progress = LessonProgress(
                lesson_id=lesson_two.id,
                user_id=users["student"].id,
                time_spent_seconds=1200,
                is_completed=False,
                updated_at=_now(),
            )
            session.add(progress)

        event_start = _now() + timedelta(days=1)
        result = await session.execute(
            select(CalendarEvent).where(
                CalendarEvent.title == "Live Q&A Session",
                CalendarEvent.course_id == course_fastapi.id,
            )
        )
        event = result.scalars().first()
        if event is None:
            session.add(
                CalendarEvent(
                    course_id=course_fastapi.id,
                    title="Live Q&A Session",
                    description="Instructor-led Q&A for module one.",
                    starts_at=event_start,
                    ends_at=event_start + timedelta(hours=1),
                    created_by=users["instructor"].id,
                    created_at=_now(),
                )
            )

        await _get_or_create_by_field(
            session,
            LmsNotification,
            "title",
            "Assessment Graded",
            user_id=users["student"].id,
            message="Your FastAPI Basics Quiz has been graded.",
            type="assessment",
            is_read=False,
            read_at=None,
            created_at=_now(),
        )

        await session.commit()

    print("LMS QA seed complete.")
    print("QA accounts:")
    print("- admin@lms-qa.example.com / Admin123!")
    print("- instructor@lms-qa.example.com / Instructor123!")
    print("- student@lms-qa.example.com / Student123!")
    print("- student2@lms-qa.example.com / Student123!")
    print("- parent@lms-qa.example.com / Parent123!")


if __name__ == "__main__":
    asyncio.run(seed())
