from __future__ import annotations

from datetime import date, datetime
from typing import Annotated, Any, Optional
import os
import uuid

from fastapi import APIRouter, Depends, File, HTTPException, Query, Request, UploadFile, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from pydantic import BaseModel, EmailStr, Field
from sqlalchemy import func
from sqlalchemy.ext.asyncio import AsyncSession

from src.apps.core.security import (
    TokenValidationError,
    create_access_token,
    create_refresh_token,
    decode_token,
    get_password_hash,
    verify_password,
)
from src.apps.iam.models.user import User, UserProfile
from src.db.query import select
from src.db.session import get_session

from .models import (
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


router = APIRouter(tags=["LMS"])
_bearer_scheme = HTTPBearer(auto_error=False)


class RegisterRequest(BaseModel):
    username: str
    email: EmailStr
    password: str = Field(min_length=8)


class LoginRequest(BaseModel):
    email: EmailStr
    password: str


class ManageInfoRequest(BaseModel):
    first_name: str = ""
    last_name: str = ""
    phone: str = ""
    bio: str = ""


class CourseIn(BaseModel):
    title: str
    description: str = ""
    category: str = "General"
    level: str = "Beginner"
    instructor_id: int | None = None


class ModuleIn(BaseModel):
    title: str
    description: str = ""
    order_index: int = 0


class LessonIn(BaseModel):
    title: str
    content: str = ""
    duration_minutes: int = 0


class LessonProgressIn(BaseModel):
    timeSpent: str = "00:00:00"
    isCompleted: bool = False


class EnrollmentIn(BaseModel):
    userId: int


class AnnouncementIn(BaseModel):
    title: str
    content: str
    courseId: int | None = None


class AssessmentSubmitIn(BaseModel):
    responses: dict[str, Any] = Field(default_factory=dict)


class GradeAttemptIn(BaseModel):
    manualScores: dict[str, int] = Field(default_factory=dict)


class AttendanceItemIn(BaseModel):
    userId: int
    status: str = "present"
    notes: str = ""


class AttendanceBatchIn(BaseModel):
    courseId: int
    sessionDate: date
    items: list[AttendanceItemIn]


class CompetencyIn(BaseModel):
    name: str
    description: str = ""


class LinkCompetencyIn(BaseModel):
    competencyId: int


class ParentLinkIn(BaseModel):
    parentId: int


class ApplicationIn(BaseModel):
    programId: str
    payload: dict[str, Any] = Field(default_factory=dict)


class DecisionIn(BaseModel):
    status: str
    reviewerNotes: str = ""


class ForumPostIn(BaseModel):
    content: str


class ConversationIn(BaseModel):
    participantIds: list[int]
    title: str = ""


class MessageIn(BaseModel):
    conversationId: int
    encryptedContent: str


class CriteriaIn(BaseModel):
    criterion: dict[str, Any]


class AchievementIn(BaseModel):
    name: str
    description: str = ""
    points: int = 0


class CategoryIn(BaseModel):
    name: str
    parentId: int | None = None


class TagIn(BaseModel):
    name: str


class UserUpdateIn(BaseModel):
    username: str | None = None
    email: EmailStr | None = None
    is_active: bool | None = None


class DeleteBatchIn(BaseModel):
    fileIds: list[int]
    onConflict: str | None = None


class CalendarEventIn(BaseModel):
    courseId: int
    title: str
    description: str = ""
    startsAt: datetime
    endsAt: datetime


class NoteIn(BaseModel):
    title: str = ""
    content: str
    courseId: int | None = None
    lessonId: int | None = None
    tag: str = ""
    priority: str = "normal"
    isPinned: bool = False
    attachmentFileId: int | None = None


class NoteUpdateIn(BaseModel):
    title: str | None = None
    content: str | None = None
    courseId: int | None = None
    lessonId: int | None = None
    tag: str | None = None
    priority: str | None = None
    isPinned: bool | None = None
    attachmentFileId: int | None = None


def _serialize(obj: Any) -> dict[str, Any]:
    if obj is None:
        return {}
    return {column.name: getattr(obj, column.name) for column in obj.__table__.columns}


def _parse_timespan(raw: str) -> int:
    try:
        hours, minutes, seconds = [int(part) for part in raw.split(":")]
    except Exception as exc:  # pragma: no cover - defensive
        raise HTTPException(status_code=400, detail="timeSpent must be HH:MM:SS") from exc
    return (hours * 3600) + (minutes * 60) + seconds


async def _get_current_user(
    request: Request,
    credentials: Annotated[Optional[HTTPAuthorizationCredentials], Depends(_bearer_scheme)],
    db: Annotated[AsyncSession, Depends(get_session)],
) -> User:
    token = credentials.credentials if credentials else None
    if not token:
        token = request.cookies.get("access_token")

    if not token:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Not authenticated")

    try:
        payload = decode_token(token)
        subject = int(str(payload.get("sub", "0")))
    except (TokenValidationError, ValueError):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid token")

    result = await db.execute(select(User).where(User.id == subject))
    user = result.scalars().first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return user


def _role_names(user: User) -> set[str]:
    names: set[str] = set()
    try:
        for user_role in user.user_roles:
            if user_role.role and user_role.role.name:
                names.add(user_role.role.name.lower())
    except Exception:
        # Role relationship might not be loaded in some async execution paths.
        return names
    return names


def _require_admin(user: User) -> None:
    if user.is_superuser:
        return
    if "admin" in _role_names(user):
        return
    raise HTTPException(status_code=403, detail="Admin access required")


def _require_admin_or_instructor(user: User) -> None:
    if user.is_superuser:
        return
    names = _role_names(user)
    if "admin" in names or "instructor" in names:
        return
    raise HTTPException(status_code=403, detail="Admin or instructor access required")


async def _require_course_admin_or_instructor(user: User, course_id: int, db: AsyncSession) -> None:
    if user.is_superuser:
        return
    role_names = _role_names(user)
    if "admin" in role_names:
        return

    assignment = (
        await db.execute(select(Course).where(Course.id == course_id, Course.instructor_id == user.id))
    ).scalars().first()
    if assignment:
        return

    enrollment = (
        await db.execute(
            select(Enrollment).where(
                Enrollment.course_id == course_id,
                Enrollment.user_id == user.id,
                Enrollment.role == "instructor",
            )
        )
    ).scalars().first()
    if enrollment:
        return

    if "instructor" in role_names:
        raise HTTPException(status_code=403, detail="Only the assigned instructor can access this course")

    raise HTTPException(status_code=403, detail="Admin or course instructor access required")


async def _paginate(stmt, page: int, page_size: int, db: AsyncSession) -> dict[str, Any]:
    count_stmt = select(func.count()).select_from(stmt.subquery())
    total = (await db.execute(count_stmt)).scalar_one()
    rows = (await db.execute(stmt.offset((page - 1) * page_size).limit(page_size))).scalars().all()
    return {
        "items": [_serialize(item) for item in rows],
        "page": page,
        "pageSize": page_size,
        "total": total,
    }


# Authentication endpoints in docs
@router.post("/register")
async def register(payload: RegisterRequest, db: Annotated[AsyncSession, Depends(get_session)]):
    existing = await db.execute(select(User).where((User.email == payload.email) | (User.username == payload.username)))
    if existing.scalars().first():
        raise HTTPException(status_code=409, detail="User already exists")

    user = User(
        username=payload.username,
        email=str(payload.email),
        hashed_password=get_password_hash(payload.password),
    )
    db.add(user)
    await db.commit()
    await db.refresh(user)
    return {"id": user.id, "username": user.username, "email": user.email}


@router.post("/login")
async def login(payload: LoginRequest, db: Annotated[AsyncSession, Depends(get_session)]):
    result = await db.execute(select(User).where(User.email == str(payload.email)))
    user = result.scalars().first()
    if not user or not user.hashed_password or not verify_password(payload.password, user.hashed_password):
        raise HTTPException(status_code=401, detail="Invalid credentials")

    return {
        "access_token": create_access_token(str(user.id)),
        "refresh_token": create_refresh_token(str(user.id)),
        "token_type": "bearer",
        "user": {"id": user.id, "username": user.username, "email": user.email},
    }


@router.post("/logout")
async def logout():
    return {"message": "Logged out"}


@router.post("/manage/info")
async def manage_info(
    payload: ManageInfoRequest,
    db: Annotated[AsyncSession, Depends(get_session)],
    user: Annotated[User, Depends(_get_current_user)],
):
    result = await db.execute(select(UserProfile).where(UserProfile.user_id == user.id))
    profile = result.scalars().first()
    if not profile:
        profile = UserProfile(user_id=user.id)
        db.add(profile)

    profile.first_name = payload.first_name
    profile.last_name = payload.last_name
    profile.phone = payload.phone
    profile.bio = payload.bio
    await db.commit()
    return {"message": "Profile updated"}


# Courses
@router.get("/api/courses")
async def list_courses(
    page: int = Query(default=1, ge=1),
    pageSize: int = Query(default=10, ge=1, le=100),
    search: str | None = None,
    category: str | None = None,
    level: str | None = None,
    db: AsyncSession = Depends(get_session),
):
    stmt = select(Course)
    if search:
        stmt = stmt.where(Course.title.ilike(f"%{search}%"))
    if category:
        stmt = stmt.where(Course.category == category)
    if level:
        stmt = stmt.where(Course.level == level)
    return await _paginate(stmt.order_by(Course.id.desc()), page, pageSize, db)


@router.get("/api/courses/{id}")
async def get_course(id: int, db: AsyncSession = Depends(get_session)):
    course = (await db.execute(select(Course).where(Course.id == id))).scalars().first()
    if not course:
        raise HTTPException(status_code=404, detail="Course not found")
    return _serialize(course)


@router.post("/api/courses", status_code=201)
async def create_course(
    payload: CourseIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    course = Course(**payload.model_dump())
    db.add(course)
    await db.commit()
    await db.refresh(course)
    return _serialize(course)


@router.put("/api/courses/{id}", status_code=204)
async def update_course(
    id: int,
    payload: CourseIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    course = (await db.execute(select(Course).where(Course.id == id))).scalars().first()
    if not course:
        raise HTTPException(status_code=404, detail="Course not found")
    for key, value in payload.model_dump().items():
        setattr(course, key, value)
    course.updated_at = datetime.utcnow()
    await db.commit()


@router.delete("/api/courses/{id}", status_code=204)
async def delete_course(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    course = (await db.execute(select(Course).where(Course.id == id))).scalars().first()
    if not course:
        raise HTTPException(status_code=404, detail="Course not found")

    module_ids = (await db.execute(select(Module.id).where(Module.course_id == id))).scalars().all()
    lesson_ids = (
        await db.execute(select(Lesson.id).where(Lesson.module_id.in_(module_ids)) if module_ids else select(Lesson.id).where(Lesson.id == -1))
    ).scalars().all()
    assessment_ids = (await db.execute(select(Assessment.id).where(Assessment.course_id == id))).scalars().all()

    if lesson_ids:
        for row in (await db.execute(select(LessonProgress).where(LessonProgress.lesson_id.in_(lesson_ids)))).scalars().all():
            await db.delete(row)
        for row in (await db.execute(select(Note).where(Note.lesson_id.in_(lesson_ids)))).scalars().all():
            await db.delete(row)

    if assessment_ids:
        for row in (await db.execute(select(AssessmentAttempt).where(AssessmentAttempt.assessment_id.in_(assessment_ids)))).scalars().all():
            await db.delete(row)
        for row in (await db.execute(select(AssessmentQuestion).where(AssessmentQuestion.assessment_id.in_(assessment_ids)))).scalars().all():
            await db.delete(row)
        for row in (await db.execute(select(AssessmentCompetency).where(AssessmentCompetency.assessment_id.in_(assessment_ids)))).scalars().all():
            await db.delete(row)

    for row in (await db.execute(select(Enrollment).where(Enrollment.course_id == id))).scalars().all():
        await db.delete(row)
    for row in (await db.execute(select(Announcement).where(Announcement.course_id == id))).scalars().all():
        await db.delete(row)
    for row in (await db.execute(select(Certificate).where(Certificate.course_id == id))).scalars().all():
        await db.delete(row)
    for row in (await db.execute(select(AttendanceRecord).where(AttendanceRecord.course_id == id))).scalars().all():
        await db.delete(row)
    for row in (await db.execute(select(CourseCompetency).where(CourseCompetency.course_id == id))).scalars().all():
        await db.delete(row)
    for row in (await db.execute(select(CourseTag).where(CourseTag.course_id == id))).scalars().all():
        await db.delete(row)
    topic_ids = (await db.execute(select(ForumTopic.id).where(ForumTopic.course_id == id))).scalars().all()
    if topic_ids:
        for row in (await db.execute(select(ForumPost).where(ForumPost.topic_id.in_(topic_ids)))).scalars().all():
            await db.delete(row)
        for row in (await db.execute(select(ForumTopic).where(ForumTopic.id.in_(topic_ids)))).scalars().all():
            await db.delete(row)
    for row in (await db.execute(select(CalendarEvent).where(CalendarEvent.course_id == id))).scalars().all():
        await db.delete(row)
    for row in (await db.execute(select(Note).where(Note.course_id == id))).scalars().all():
        await db.delete(row)

    if module_ids:
        for row in (await db.execute(select(Lesson).where(Lesson.module_id.in_(module_ids)))).scalars().all():
            await db.delete(row)
        for row in (await db.execute(select(Module).where(Module.id.in_(module_ids)))).scalars().all():
            await db.delete(row)

    if assessment_ids:
        for row in (await db.execute(select(Assessment).where(Assessment.id.in_(assessment_ids)))).scalars().all():
            await db.delete(row)

    await db.delete(course)
    await db.commit()


# Modules and lessons
@router.get("/api/courses/{courseId}/modules")
async def get_modules(courseId: int, db: AsyncSession = Depends(get_session)):
    rows = (await db.execute(select(Module).where(Module.course_id == courseId).order_by(Module.order_index))).scalars().all()
    return [_serialize(row) for row in rows]


@router.get("/api/courses/{courseId}/assessments")
async def list_course_assessments(courseId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    enrollment = (
        await db.execute(select(Enrollment).where(Enrollment.course_id == courseId, Enrollment.user_id == user.id))
    ).scalars().first()
    if not enrollment and not user.is_superuser and courseId not in (
        await db.execute(select(Course.id).where(Course.id == courseId, Course.instructor_id == user.id))
    ).scalars().all():
        raise HTTPException(status_code=403, detail="Course access required")

    rows = (
        await db.execute(select(Assessment).where(Assessment.course_id == courseId).order_by(Assessment.id.desc()))
    ).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/courses/{courseId}/modules", status_code=201)
async def create_module(
    courseId: int,
    payload: ModuleIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    row = Module(course_id=courseId, **payload.model_dump())
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.put("/api/modules/{id}", status_code=204)
async def update_module(id: int, payload: ModuleIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Module).where(Module.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Module not found")
    for key, value in payload.model_dump().items():
        setattr(row, key, value)
    await db.commit()


@router.delete("/api/modules/{id}", status_code=204)
async def delete_module(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Module).where(Module.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Module not found")

    lesson_ids = (await db.execute(select(Lesson.id).where(Lesson.module_id == id))).scalars().all()
    if lesson_ids:
        for progress in (await db.execute(select(LessonProgress).where(LessonProgress.lesson_id.in_(lesson_ids)))).scalars().all():
            await db.delete(progress)
        for note in (await db.execute(select(Note).where(Note.lesson_id.in_(lesson_ids)))).scalars().all():
            await db.delete(note)
        for lesson in (await db.execute(select(Lesson).where(Lesson.id.in_(lesson_ids)))).scalars().all():
            await db.delete(lesson)

    await db.delete(row)
    await db.commit()


@router.get("/api/modules/{moduleId}/lessons")
async def get_lessons(moduleId: int, db: AsyncSession = Depends(get_session)):
    rows = (await db.execute(select(Lesson).where(Lesson.module_id == moduleId).order_by(Lesson.id))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/modules/{moduleId}/lessons", status_code=201)
async def create_lesson(
    moduleId: int,
    payload: LessonIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    row = Lesson(module_id=moduleId, **payload.model_dump())
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.put("/api/lessons/{id}", status_code=204)
async def update_lesson(id: int, payload: LessonIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Lesson).where(Lesson.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Lesson not found")
    for key, value in payload.model_dump().items():
        setattr(row, key, value)
    await db.commit()


@router.delete("/api/lessons/{id}", status_code=204)
async def delete_lesson(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Lesson).where(Lesson.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Lesson not found")

    for progress in (await db.execute(select(LessonProgress).where(LessonProgress.lesson_id == id))).scalars().all():
        await db.delete(progress)
    for note in (await db.execute(select(Note).where(Note.lesson_id == id))).scalars().all():
        await db.delete(note)

    await db.delete(row)
    await db.commit()


@router.get("/api/courses/{courseId}/structure")
async def course_structure(
    courseId: int,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    modules = (await db.execute(select(Module).where(Module.course_id == courseId).order_by(Module.order_index))).scalars().all()
    module_ids = [module.id for module in modules if module.id is not None]
    lessons = (
        await db.execute(select(Lesson).where(Lesson.module_id.in_(module_ids)) if module_ids else select(Lesson).where(Lesson.id == -1))
    ).scalars().all()
    lesson_ids = [lesson.id for lesson in lessons if lesson.id is not None]
    progress_rows = (
        await db.execute(
            select(LessonProgress).where(
                LessonProgress.user_id == user.id,
                LessonProgress.lesson_id.in_(lesson_ids) if lesson_ids else LessonProgress.lesson_id == -1,
            )
        )
    ).scalars().all()
    progress_map = {row.lesson_id: row for row in progress_rows}

    lessons_by_module: dict[int, list[dict[str, Any]]] = {}
    for lesson in lessons:
        progress = progress_map.get(lesson.id)
        row = _serialize(lesson)
        row["isCompleted"] = bool(progress.is_completed) if progress else False
        row["timeSpentSeconds"] = progress.time_spent_seconds if progress else 0
        lessons_by_module.setdefault(lesson.module_id, []).append(row)

    return {
        "courseId": courseId,
        "modules": [
            {
                **_serialize(module),
                "lessons": lessons_by_module.get(module.id or 0, []),
            }
            for module in modules
        ],
    }


@router.post("/api/lessons/{lessonId}/progress")
async def update_progress(
    lessonId: int,
    payload: LessonProgressIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    row = (
        await db.execute(select(LessonProgress).where(LessonProgress.lesson_id == lessonId, LessonProgress.user_id == user.id))
    ).scalars().first()
    if not row:
        row = LessonProgress(lesson_id=lessonId, user_id=user.id)
        db.add(row)

    row.time_spent_seconds = _parse_timespan(payload.timeSpent)
    row.is_completed = payload.isCompleted
    row.updated_at = datetime.utcnow()

    await db.commit()
    unlocked = []
    if payload.isCompleted:
        unlocked.append("lesson_completed")
    return {"unlockedAchievements": unlocked}


# Enrollments
@router.get("/api/courses/{courseId}/enrollments")
async def list_course_enrollments(
    courseId: int,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    await _require_course_admin_or_instructor(user, courseId, db)
    rows = (await db.execute(select(Enrollment).where(Enrollment.course_id == courseId))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/courses/{courseId}/enrollments", status_code=201)
async def create_enrollment(
    courseId: int,
    payload: EnrollmentIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    await _require_course_admin_or_instructor(user, courseId, db)
    row = Enrollment(course_id=courseId, user_id=payload.userId)
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.delete("/api/enrollments/{enrollmentId}", status_code=204)
async def delete_enrollment(
    enrollmentId: int,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin_or_instructor(user)
    row = (await db.execute(select(Enrollment).where(Enrollment.id == enrollmentId))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Enrollment not found")
    await db.delete(row)
    await db.commit()


@router.post("/api/courses/{courseId}/enroll")
async def self_enroll(courseId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    existing = (
        await db.execute(select(Enrollment).where(Enrollment.course_id == courseId, Enrollment.user_id == user.id))
    ).scalars().first()
    if existing:
        return {"message": "Already enrolled", "enrollmentId": existing.id}
    row = Enrollment(course_id=courseId, user_id=user.id)
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return {"message": "Enrolled", "enrollmentId": row.id}


@router.get("/api/my-enrollments")
async def my_enrollments(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    rows = (await db.execute(select(Enrollment).where(Enrollment.user_id == user.id))).scalars().all()
    return [_serialize(row) for row in rows]


# Gradebook and assessments
@router.get("/api/courses/{courseId}/gradebook")
async def gradebook(courseId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    await _require_course_admin_or_instructor(user, courseId, db)

    attempts = (
        await db.execute(
            select(AssessmentAttempt).join(Assessment, Assessment.id == AssessmentAttempt.assessment_id).where(Assessment.course_id == courseId)
        )
    ).scalars().all()
    summary: dict[int, dict[str, Any]] = {}
    for attempt in attempts:
        key = attempt.user_id
        entry = summary.setdefault(key, {"userId": key, "scores": [], "averageScore": 0})
        entry["scores"].append({"attemptId": attempt.id, "assessmentId": attempt.assessment_id, "score": attempt.score})

    for entry in summary.values():
        scores = [score["score"] for score in entry["scores"]]
        entry["averageScore"] = (sum(scores) / len(scores)) if scores else 0

    return list(summary.values())


@router.post("/api/assessment-attempts/{id}/grade")
async def grade_attempt(
    id: int,
    payload: GradeAttemptIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    attempt = (await db.execute(select(AssessmentAttempt).where(AssessmentAttempt.id == id))).scalars().first()
    if not attempt:
        raise HTTPException(status_code=404, detail="Attempt not found")

    assessment = (await db.execute(select(Assessment).where(Assessment.id == attempt.assessment_id))).scalars().first()
    if not assessment:
        raise HTTPException(status_code=404, detail="Assessment not found")
    await _require_course_admin_or_instructor(user, assessment.course_id, db)

    manual_total = sum(payload.manualScores.values())
    attempt.score = int(attempt.score + manual_total)
    attempt.status = "graded"
    attempt.graded_by = user.id
    await db.commit()
    return _serialize(attempt)


@router.get("/api/announcements")
async def list_announcements(courseId: int | None = None, db: AsyncSession = Depends(get_session)):
    stmt = select(Announcement)
    if courseId is not None:
        stmt = stmt.where(Announcement.course_id == courseId)
    rows = (await db.execute(stmt.order_by(Announcement.created_at.desc()))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/announcements", status_code=201)
async def create_announcement(
    payload: AnnouncementIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    row = Announcement(
        title=payload.title,
        content=payload.content,
        course_id=payload.courseId,
        created_by=user.id,
    )
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.get("/api/assessments/{id}")
async def get_assessment(id: int, db: AsyncSession = Depends(get_session)):
    assessment = (await db.execute(select(Assessment).where(Assessment.id == id))).scalars().first()
    if not assessment:
        raise HTTPException(status_code=404, detail="Assessment not found")

    questions = (await db.execute(select(AssessmentQuestion).where(AssessmentQuestion.assessment_id == id))).scalars().all()
    payload = _serialize(assessment)
    payload["questions"] = [_serialize(question) for question in questions]
    return payload


@router.get("/api/assessments/{id}/attempts/me")
async def my_assessment_attempts(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    rows = (
        await db.execute(
            select(AssessmentAttempt)
            .where(AssessmentAttempt.assessment_id == id, AssessmentAttempt.user_id == user.id)
            .order_by(AssessmentAttempt.submitted_at.desc())
        )
    ).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/assessments/{id}/submit")
async def submit_assessment(
    id: int,
    payload: AssessmentSubmitIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    questions = (await db.execute(select(AssessmentQuestion).where(AssessmentQuestion.assessment_id == id))).scalars().all()
    score = 0
    for question in questions:
        answer = payload.responses.get(str(question.id))
        if question.is_manual:
            continue
        if answer == question.correct_answer:
            score += question.points

    attempt = AssessmentAttempt(
        assessment_id=id,
        user_id=user.id,
        responses_json=payload.responses,
        score=score,
        status="submitted",
    )
    db.add(attempt)
    await db.commit()
    await db.refresh(attempt)
    return _serialize(attempt)


# Certificates and user management
@router.get("/api/certificates")
async def list_certificates(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    rows = (await db.execute(select(Certificate).order_by(Certificate.issued_at.desc()))).scalars().all()
    return [_serialize(row) for row in rows]


@router.get("/api/certificates/{id}")
async def get_certificate(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = (await db.execute(select(Certificate).where(Certificate.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Certificate not found")
    if row.user_id != user.id:
        _require_admin(user)
    return _serialize(row)


@router.get("/api/users/me/certificates")
async def my_certificates(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    rows = (await db.execute(select(Certificate).where(Certificate.user_id == user.id))).scalars().all()
    return [_serialize(row) for row in rows]


@router.get("/api/admin/users")
async def admin_users(
    page: int = Query(default=1, ge=1),
    pageSize: int = Query(default=10, ge=1, le=100),
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    stmt = select(User).order_by(User.id.desc())
    return await _paginate(stmt, page, pageSize, db)


@router.get("/api/admin/users/{id}")
async def admin_user_get(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(User).where(User.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="User not found")
    return _serialize(row)


@router.put("/api/admin/users/{id}", status_code=204)
async def admin_user_update(
    id: int,
    payload: UserUpdateIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    row = (await db.execute(select(User).where(User.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="User not found")
    for key, value in payload.model_dump(exclude_none=True).items():
        setattr(row, key, value)
    await db.commit()


@router.delete("/api/admin/users/{id}", status_code=204)
async def admin_user_delete(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(User).where(User.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="User not found")
    await db.delete(row)
    await db.commit()


# Parent portal
@router.get("/api/parent/my-children")
async def parent_children(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    links = (await db.execute(select(ParentStudentLink).where(ParentStudentLink.parent_id == user.id))).scalars().all()
    student_ids = [link.student_id for link in links]
    if not student_ids:
        return []
    students = (await db.execute(select(User).where(User.id.in_(student_ids)))).scalars().all()
    return [{"id": row.id, "username": row.username, "email": row.email} for row in students]


async def _assert_parent_link(parent_id: int, student_id: int, db: AsyncSession) -> None:
    link = (
        await db.execute(
            select(ParentStudentLink).where(
                ParentStudentLink.parent_id == parent_id,
                ParentStudentLink.student_id == student_id,
            )
        )
    ).scalars().first()
    if not link:
        raise HTTPException(status_code=403, detail="Parent is not linked to this student")


@router.get("/api/parent/children/{studentId}/dashboard")
async def child_dashboard(studentId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    await _assert_parent_link(user.id, studentId, db)
    enroll_count = (
        await db.execute(select(func.count()).select_from(Enrollment).where(Enrollment.user_id == studentId))
    ).scalar_one()
    attempt_count = (
        await db.execute(select(func.count()).select_from(AssessmentAttempt).where(AssessmentAttempt.user_id == studentId))
    ).scalar_one()
    return {"studentId": studentId, "enrollments": enroll_count, "assessmentAttempts": attempt_count}


@router.get("/api/parent/children/{studentId}/grades")
async def child_grades(studentId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    await _assert_parent_link(user.id, studentId, db)
    rows = (await db.execute(select(AssessmentAttempt).where(AssessmentAttempt.user_id == studentId))).scalars().all()
    return [_serialize(row) for row in rows]


@router.get("/api/parent/children/{studentId}/attendance")
async def child_attendance(studentId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    await _assert_parent_link(user.id, studentId, db)
    rows = (await db.execute(select(AttendanceRecord).where(AttendanceRecord.user_id == studentId))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/admin/students/{studentId}/parent-links", status_code=204)
async def add_parent_link(
    studentId: int,
    payload: ParentLinkIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    row = ParentStudentLink(parent_id=payload.parentId, student_id=studentId)
    db.add(row)
    await db.commit()


@router.delete("/api/admin/students/{studentId}/parent-links/{parentId}", status_code=204)
async def remove_parent_link(studentId: int, parentId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (
        await db.execute(
            select(ParentStudentLink).where(
                ParentStudentLink.parent_id == parentId,
                ParentStudentLink.student_id == studentId,
            )
        )
    ).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Link not found")
    await db.delete(row)
    await db.commit()


# Admissions
@router.post("/api/admissions/apply", status_code=201)
async def apply_admission(payload: ApplicationIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = AdmissionApplication(
        user_id=user.id,
        program_id=payload.programId,
        payload_json=payload.payload,
    )
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.get("/api/admissions/my-application")
async def my_application(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = (
        await db.execute(select(AdmissionApplication).where(AdmissionApplication.user_id == user.id).order_by(AdmissionApplication.id.desc()))
    ).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="No application found")
    return _serialize(row)


@router.get("/api/admin/admissions/applications")
async def list_applications(
    status: str | None = None,
    programId: str | None = None,
    page: int = Query(default=1, ge=1),
    pageSize: int = Query(default=10, ge=1, le=100),
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    stmt = select(AdmissionApplication)
    if status:
        stmt = stmt.where(AdmissionApplication.status == status)
    if programId:
        stmt = stmt.where(AdmissionApplication.program_id == programId)
    return await _paginate(stmt.order_by(AdmissionApplication.id.desc()), page, pageSize, db)


@router.get("/api/admin/admissions/applications/{id}")
async def get_application(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(AdmissionApplication).where(AdmissionApplication.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Application not found")
    return _serialize(row)


@router.post("/api/admin/admissions/applications/{id}/decision", status_code=204)
async def set_application_decision(
    id: int,
    payload: DecisionIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin(user)
    row = (await db.execute(select(AdmissionApplication).where(AdmissionApplication.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Application not found")
    row.status = payload.status
    row.reviewer_notes = payload.reviewerNotes
    row.updated_at = datetime.utcnow()
    await db.commit()


# Competency
@router.get("/api/competencies")
async def list_competencies(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin_or_instructor(user)
    rows = (await db.execute(select(Competency).order_by(Competency.name))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/competencies", status_code=201)
async def create_competency(payload: CompetencyIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = Competency(**payload.model_dump())
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.post("/api/courses/{courseId}/competencies", status_code=204)
async def link_course_competency(
    courseId: int,
    payload: LinkCompetencyIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin_or_instructor(user)
    row = CourseCompetency(course_id=courseId, competency_id=payload.competencyId)
    db.add(row)
    await db.commit()


@router.post("/api/assessments/{assessmentId}/competencies", status_code=204)
async def link_assessment_competency(
    assessmentId: int,
    payload: LinkCompetencyIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin_or_instructor(user)
    row = AssessmentCompetency(assessment_id=assessmentId, competency_id=payload.competencyId)
    db.add(row)
    await db.commit()


@router.get("/api/users/{userId}/competency-profile")
async def user_competency_profile(userId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    if user.id != userId:
        _require_admin_or_instructor(user)
    rows = (await db.execute(select(UserCompetency).where(UserCompetency.user_id == userId))).scalars().all()
    return [_serialize(row) for row in rows]


# Reports
async def _ensure_report_access(course_id: int | None, db: AsyncSession, user: User) -> None:
    if user.is_superuser or "admin" in _role_names(user):
        return
    if not course_id:
        raise HTTPException(status_code=403, detail="Course filter required for non-admin users")
    assignment = (
        await db.execute(select(Course).where(Course.id == course_id, Course.instructor_id == user.id))
    ).scalars().first()
    if not assignment:
        raise HTTPException(status_code=403, detail="No access to this course report")


@router.get("/api/reports/student-engagement")
async def report_student_engagement(
    courseId: int | None = None,
    startDate: date | None = None,
    endDate: date | None = None,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    await _ensure_report_access(courseId, db, user)
    stmt = select(LessonProgress)
    if courseId:
        stmt = stmt.join(Lesson, Lesson.id == LessonProgress.lesson_id).join(Module, Module.id == Lesson.module_id).where(Module.course_id == courseId)
    rows = (await db.execute(stmt)).scalars().all()
    total_time = sum(row.time_spent_seconds for row in rows)
    completed = sum(1 for row in rows if row.is_completed)
    return {
        "filters": {"courseId": courseId, "startDate": startDate, "endDate": endDate},
        "metrics": {"events": len(rows), "totalTimeSeconds": total_time, "completedLessons": completed},
    }


@router.get("/api/reports/course-completion")
async def report_course_completion(
    courseId: int | None = None,
    startDate: date | None = None,
    endDate: date | None = None,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    await _ensure_report_access(courseId, db, user)
    enroll_stmt = select(Enrollment)
    if courseId:
        enroll_stmt = enroll_stmt.where(Enrollment.course_id == courseId)
    enrollments = (await db.execute(enroll_stmt)).scalars().all()

    completion_data = []
    for enrollment in enrollments:
        course_modules = (await db.execute(select(Module.id).where(Module.course_id == enrollment.course_id))).scalars().all()
        module_ids = [module_id for module_id in course_modules]
        lesson_ids = (await db.execute(select(Lesson.id).where(Lesson.module_id.in_(module_ids)) if module_ids else select(Lesson.id).where(Lesson.id == -1))).scalars().all()
        completed = (
            await db.execute(
                select(func.count()).select_from(LessonProgress).where(
                    LessonProgress.user_id == enrollment.user_id,
                    LessonProgress.is_completed,
                    LessonProgress.lesson_id.in_(lesson_ids) if lesson_ids else LessonProgress.lesson_id == -1,
                )
            )
        ).scalar_one()
        total = len(lesson_ids)
        completion_data.append(
            {
                "courseId": enrollment.course_id,
                "userId": enrollment.user_id,
                "totalLessons": total,
                "completedLessons": completed,
                "completionRate": round((completed / total) * 100, 2) if total else 0,
            }
        )

    return {
        "filters": {"courseId": courseId, "startDate": startDate, "endDate": endDate},
        "items": completion_data,
    }


# Attendance
@router.post("/api/attendance")
async def attendance_batch(
    payload: AttendanceBatchIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    await _require_course_admin_or_instructor(user, payload.courseId, db)
    created = 0
    for item in payload.items:
        row = AttendanceRecord(
            course_id=payload.courseId,
            user_id=item.userId,
            session_date=payload.sessionDate,
            status=item.status,
            notes=item.notes,
            marked_by=user.id,
        )
        db.add(row)
        created += 1
    await db.commit()
    return {"created": created}


@router.get("/api/courses/{courseId}/attendance")
async def course_attendance(courseId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    await _require_course_admin_or_instructor(user, courseId, db)
    rows = (await db.execute(select(AttendanceRecord).where(AttendanceRecord.course_id == courseId))).scalars().all()
    return [_serialize(row) for row in rows]


@router.get("/api/users/{userId}/attendance")
async def user_attendance(userId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    if user.id != userId:
        _require_admin(user)
    rows = (await db.execute(select(AttendanceRecord).where(AttendanceRecord.user_id == userId))).scalars().all()
    return [_serialize(row) for row in rows]


@router.put("/api/attendance/{id}")
async def update_attendance(
    id: int,
    payload: AttendanceItemIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin_or_instructor(user)
    row = (await db.execute(select(AttendanceRecord).where(AttendanceRecord.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Attendance record not found")
    row.user_id = payload.userId
    row.status = payload.status
    row.notes = payload.notes
    row.marked_by = user.id
    await db.commit()
    return _serialize(row)


# Gamification
@router.post("/api/achievements", status_code=201)
async def create_achievement(payload: AchievementIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin_or_instructor(user)
    row = Achievement(**payload.model_dump())
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.post("/api/achievements/{id}/criteria")
async def add_achievement_criteria(
    id: int,
    payload: CriteriaIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    _require_admin_or_instructor(user)
    row = (await db.execute(select(Achievement).where(Achievement.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Achievement not found")
    criteria = list(row.criteria_json or [])
    criteria.append(payload.criterion)
    row.criteria_json = criteria
    await db.commit()
    return _serialize(row)


@router.get("/api/leaderboard")
async def leaderboard(db: AsyncSession = Depends(get_session)):
    rows = (
        await db.execute(
            select(
                UserAchievement.user_id,
                func.coalesce(func.sum(Achievement.points), 0).label("points"),
            )
            .join(Achievement, Achievement.id == UserAchievement.achievement_id)
            .group_by(UserAchievement.user_id)
            .order_by(func.sum(Achievement.points).desc())
        )
    ).all()
    return [{"userId": row.user_id, "points": row.points} for row in rows]


@router.get("/api/users/{id}/achievements")
async def user_achievements(id: int, db: AsyncSession = Depends(get_session)):
    rows = (await db.execute(select(UserAchievement).where(UserAchievement.user_id == id))).scalars().all()
    return [_serialize(row) for row in rows]


# Categories and tags
@router.get("/api/categories")
async def list_categories(db: AsyncSession = Depends(get_session)):
    rows = (await db.execute(select(Category).order_by(Category.name))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/categories", status_code=201)
async def create_category(payload: CategoryIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = Category(name=payload.name, parent_id=payload.parentId)
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.put("/api/categories/{id}")
async def update_category(id: int, payload: CategoryIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Category).where(Category.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Category not found")
    row.name = payload.name
    row.parent_id = payload.parentId
    await db.commit()
    return _serialize(row)


@router.delete("/api/categories/{id}", status_code=204)
async def delete_category(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Category).where(Category.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Category not found")
    await db.delete(row)
    await db.commit()


@router.get("/api/tags")
async def list_tags(db: AsyncSession = Depends(get_session)):
    rows = (await db.execute(select(Tag).order_by(Tag.name))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/tags", status_code=201)
async def create_tag(payload: TagIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = Tag(name=payload.name)
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.put("/api/tags/{id}")
async def update_tag(id: int, payload: TagIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Tag).where(Tag.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Tag not found")
    row.name = payload.name
    await db.commit()
    return _serialize(row)


@router.delete("/api/tags/{id}", status_code=204)
async def delete_tag(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin(user)
    row = (await db.execute(select(Tag).where(Tag.id == id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Tag not found")
    await db.delete(row)
    await db.commit()


# Communication
@router.post("/api/conversations", status_code=201)
async def create_conversation(
    payload: ConversationIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    row = Conversation(title=payload.title, is_group=len(payload.participantIds) > 1, created_by=user.id)
    db.add(row)
    await db.flush()

    participant_ids = set(payload.participantIds)
    participant_ids.add(user.id)
    for participant_id in participant_ids:
        db.add(ConversationParticipant(conversation_id=row.id, user_id=participant_id))

    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.get("/api/conversations")
async def list_conversations(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    memberships = (
        await db.execute(select(ConversationParticipant).where(ConversationParticipant.user_id == user.id))
    ).scalars().all()
    conversation_ids = [item.conversation_id for item in memberships]
    if not conversation_ids:
        return []
    rows = (await db.execute(select(Conversation).where(Conversation.id.in_(conversation_ids)))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/messages", status_code=201)
async def send_message(payload: MessageIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    membership = (
        await db.execute(
            select(ConversationParticipant).where(
                ConversationParticipant.conversation_id == payload.conversationId,
                ConversationParticipant.user_id == user.id,
            )
        )
    ).scalars().first()
    if not membership:
        raise HTTPException(status_code=403, detail="Not a conversation participant")

    row = Message(
        conversation_id=payload.conversationId,
        sender_id=user.id,
        encrypted_content=payload.encryptedContent,
    )
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.get("/api/conversations/{id}/messages")
async def list_messages(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    membership = (
        await db.execute(
            select(ConversationParticipant).where(
                ConversationParticipant.conversation_id == id,
                ConversationParticipant.user_id == user.id,
            )
        )
    ).scalars().first()
    if not membership:
        raise HTTPException(status_code=403, detail="Not a conversation participant")

    rows = (await db.execute(select(Message).where(Message.conversation_id == id).order_by(Message.created_at))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/forum-topics/{id}/posts", status_code=201)
async def create_forum_post(id: int, payload: ForumPostIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    topic = (await db.execute(select(ForumTopic).where(ForumTopic.id == id))).scalars().first()
    if not topic:
        raise HTTPException(status_code=404, detail="Forum topic not found")
    row = ForumPost(topic_id=id, user_id=user.id, content=payload.content)
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


# File Management
@router.get("/api/my-files")
async def my_files(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    rows = (await db.execute(select(MediaFile).where(MediaFile.user_id == user.id).order_by(MediaFile.uploaded_at.desc()))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/my-files", status_code=201)
async def upload_my_file(
    file: UploadFile = File(...),
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    os.makedirs("media/lms", exist_ok=True)
    suffix = ""
    if file.filename and "." in file.filename:
        suffix = "." + file.filename.rsplit(".", 1)[-1]
    stored_name = f"{uuid.uuid4().hex}{suffix}"
    rel_path = f"media/lms/{stored_name}"

    data = await file.read()
    with open(rel_path, "wb") as output:
        output.write(data)

    row = MediaFile(
        user_id=user.id,
        original_name=file.filename or stored_name,
        stored_name=stored_name,
        content_type=file.content_type or "application/octet-stream",
        size_bytes=len(data),
        storage_path=rel_path,
    )
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


def _file_reference_count_stmt(file_id: int):
    return select(func.count()).select_from(Note).where(Note.attachment_file_id == file_id)


@router.delete("/api/my-files/{id}", status_code=204)
async def delete_my_file(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = (await db.execute(select(MediaFile).where(MediaFile.id == id, MediaFile.user_id == user.id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="File not found")

    in_use = (await db.execute(_file_reference_count_stmt(id))).scalar_one()
    if in_use > 0:
        raise HTTPException(status_code=409, detail="File is in use")

    if os.path.exists(row.storage_path):
        os.remove(row.storage_path)
    await db.delete(row)
    await db.commit()


@router.post("/api/my-files/delete-batch")
async def delete_my_files_batch(
    payload: DeleteBatchIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    result = {
        "deleted": [],
        "conflicts": [],
    }
    for file_id in payload.fileIds:
        row = (await db.execute(select(MediaFile).where(MediaFile.id == file_id, MediaFile.user_id == user.id))).scalars().first()
        if not row:
            continue

        in_use = (await db.execute(_file_reference_count_stmt(file_id))).scalar_one()
        if in_use > 0 and payload.onConflict != "setNull":
            result["conflicts"].append({"fileId": file_id, "reason": "in_use"})
            continue

        if in_use > 0 and payload.onConflict == "setNull":
            notes = (await db.execute(select(Note).where(Note.attachment_file_id == file_id))).scalars().all()
            for note in notes:
                note.attachment_file_id = None

        if os.path.exists(row.storage_path):
            os.remove(row.storage_path)
        await db.delete(row)
        result["deleted"].append(file_id)

    await db.commit()
    return result


# Notifications & Calendar
@router.get("/api/notifications")
async def my_notifications(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    rows = (await db.execute(select(LmsNotification).where(LmsNotification.user_id == user.id).order_by(LmsNotification.created_at.desc()))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/notifications/{id}/mark-as-read")
async def mark_notification_read(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = (await db.execute(select(LmsNotification).where(LmsNotification.id == id, LmsNotification.user_id == user.id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Notification not found")
    row.is_read = True
    row.read_at = datetime.utcnow()
    await db.commit()
    return _serialize(row)


@router.get("/api/calendar")
async def my_calendar(
    startDate: datetime,
    endDate: datetime,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    enrolled_course_ids = (await db.execute(select(Enrollment.course_id).where(Enrollment.user_id == user.id))).scalars().all()
    stmt = select(CalendarEvent).where(CalendarEvent.starts_at <= endDate, CalendarEvent.ends_at >= startDate)
    if enrolled_course_ids:
        stmt = stmt.where((CalendarEvent.course_id.in_(enrolled_course_ids)) | (CalendarEvent.created_by == user.id))
    else:
        stmt = stmt.where(CalendarEvent.created_by == user.id)
    rows = (await db.execute(stmt.order_by(CalendarEvent.starts_at))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/calendar-events", status_code=201)
async def create_calendar_event(
    payload: CalendarEventIn,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    await _require_course_admin_or_instructor(user, payload.courseId, db)
    if payload.endsAt <= payload.startsAt:
        raise HTTPException(status_code=400, detail="endsAt must be after startsAt")

    row = CalendarEvent(
        course_id=payload.courseId,
        title=payload.title,
        description=payload.description,
        starts_at=payload.startsAt,
        ends_at=payload.endsAt,
        created_by=user.id,
    )
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


# Dropdowns
@router.get("/api/dropdown/categories")
async def dropdown_categories(db: AsyncSession = Depends(get_session)):
    rows = (await db.execute(select(Category).order_by(Category.name))).scalars().all()
    return [{"value": row.id, "label": row.name} for row in rows]


@router.get("/api/dropdown/course-levels")
async def dropdown_course_levels():
    levels = ["Beginner", "Intermediate", "Advanced", "Expert"]
    return [{"value": level, "label": level} for level in levels]


@router.get("/api/dropdown/users")
async def dropdown_users(db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    _require_admin_or_instructor(user)
    rows = (await db.execute(select(User).where(User.is_active).order_by(User.username))).scalars().all()
    return [{"value": row.id, "label": row.username} for row in rows]


# Notes
@router.get("/api/notes")
async def list_notes(
    courseId: int | None = None,
    lessonId: int | None = None,
    searchTerm: str | None = None,
    tag: str | None = None,
    priority: str | None = None,
    isPinned: bool | None = None,
    db: AsyncSession = Depends(get_session),
    user: User = Depends(_get_current_user),
):
    stmt = select(Note).where(Note.user_id == user.id)
    if courseId is not None:
        stmt = stmt.where(Note.course_id == courseId)
    if lessonId is not None:
        stmt = stmt.where(Note.lesson_id == lessonId)
    if searchTerm:
        stmt = stmt.where((Note.title.ilike(f"%{searchTerm}%")) | (Note.content.ilike(f"%{searchTerm}%")))
    if tag:
        stmt = stmt.where(Note.tag == tag)
    if priority:
        stmt = stmt.where(Note.priority == priority)
    if isPinned is not None:
        stmt = stmt.where(Note.is_pinned == isPinned)
    rows = (await db.execute(stmt.order_by(Note.is_pinned.desc(), Note.updated_at.desc()))).scalars().all()
    return [_serialize(row) for row in rows]


@router.get("/api/lessons/{lessonId}/notes")
async def lesson_notes(lessonId: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    rows = (await db.execute(select(Note).where(Note.user_id == user.id, Note.lesson_id == lessonId).order_by(Note.updated_at.desc()))).scalars().all()
    return [_serialize(row) for row in rows]


@router.post("/api/notes", status_code=201)
async def create_note(payload: NoteIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    if payload.attachmentFileId is not None:
        attachment = (
            await db.execute(select(MediaFile).where(MediaFile.id == payload.attachmentFileId, MediaFile.user_id == user.id))
        ).scalars().first()
        if not attachment:
            raise HTTPException(status_code=400, detail="attachmentFileId is invalid for current user")

    row = Note(
        user_id=user.id,
        course_id=payload.courseId,
        lesson_id=payload.lessonId,
        title=payload.title,
        content=payload.content,
        tag=payload.tag,
        priority=payload.priority,
        is_pinned=payload.isPinned,
        attachment_file_id=payload.attachmentFileId,
    )
    db.add(row)
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.put("/api/notes/{id}")
async def update_note(id: int, payload: NoteUpdateIn, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = (await db.execute(select(Note).where(Note.id == id, Note.user_id == user.id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Note not found")

    data = payload.model_dump(exclude_unset=True)
    if "attachmentFileId" in data and data["attachmentFileId"] is not None:
        attachment = (
            await db.execute(select(MediaFile).where(MediaFile.id == data["attachmentFileId"], MediaFile.user_id == user.id))
        ).scalars().first()
        if not attachment:
            raise HTTPException(status_code=400, detail="attachmentFileId is invalid for current user")

    mapping = {
        "title": "title",
        "content": "content",
        "courseId": "course_id",
        "lessonId": "lesson_id",
        "tag": "tag",
        "priority": "priority",
        "isPinned": "is_pinned",
        "attachmentFileId": "attachment_file_id",
    }
    for source, target in mapping.items():
        if source in data:
            setattr(row, target, data[source])
    row.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(row)
    return _serialize(row)


@router.delete("/api/notes/{id}", status_code=204)
async def delete_note(id: int, db: AsyncSession = Depends(get_session), user: User = Depends(_get_current_user)):
    row = (await db.execute(select(Note).where(Note.id == id, Note.user_id == user.id))).scalars().first()
    if not row:
        raise HTTPException(status_code=404, detail="Note not found")
    await db.delete(row)
    await db.commit()
