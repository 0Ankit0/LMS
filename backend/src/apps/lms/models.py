from __future__ import annotations

from datetime import date, datetime
from typing import Optional

from sqlalchemy import JSON, Date, DateTime, ForeignKey, Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column

from src.db.base import Base


class Course(Base):
    __tablename__ = "lms_course"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    title: Mapped[str] = mapped_column(String(255), index=True)
    description: Mapped[str] = mapped_column(Text, default="")
    category: Mapped[str] = mapped_column(String(120), default="General", index=True)
    level: Mapped[str] = mapped_column(String(50), default="Beginner", index=True)
    instructor_id: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)
    updated_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Module(Base):
    __tablename__ = "lms_module"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    title: Mapped[str] = mapped_column(String(255))
    description: Mapped[str] = mapped_column(Text, default="")
    order_index: Mapped[int] = mapped_column(Integer, default=0)


class Lesson(Base):
    __tablename__ = "lms_lesson"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    module_id: Mapped[int] = mapped_column(ForeignKey("lms_module.id"), index=True)
    title: Mapped[str] = mapped_column(String(255))
    content: Mapped[str] = mapped_column(Text, default="")
    duration_minutes: Mapped[int] = mapped_column(Integer, default=0)


class LessonProgress(Base):
    __tablename__ = "lms_lesson_progress"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    lesson_id: Mapped[int] = mapped_column(ForeignKey("lms_lesson.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    time_spent_seconds: Mapped[int] = mapped_column(Integer, default=0)
    is_completed: Mapped[bool] = mapped_column(default=False)
    updated_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Enrollment(Base):
    __tablename__ = "lms_enrollment"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    role: Mapped[str] = mapped_column(String(40), default="student")
    enrolled_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Announcement(Base):
    __tablename__ = "lms_announcement"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    title: Mapped[str] = mapped_column(String(255))
    content: Mapped[str] = mapped_column(Text)
    course_id: Mapped[Optional[int]] = mapped_column(ForeignKey("lms_course.id"), default=None, index=True)
    created_by: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Assessment(Base):
    __tablename__ = "lms_assessment"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    title: Mapped[str] = mapped_column(String(255))
    description: Mapped[str] = mapped_column(Text, default="")
    max_score: Mapped[int] = mapped_column(Integer, default=100)


class AssessmentQuestion(Base):
    __tablename__ = "lms_assessment_question"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    assessment_id: Mapped[int] = mapped_column(ForeignKey("lms_assessment.id"), index=True)
    prompt: Mapped[str] = mapped_column(Text)
    options_json: Mapped[list[str]] = mapped_column(JSON, default_factory=list)
    correct_answer: Mapped[str] = mapped_column(String(255), default="")
    points: Mapped[int] = mapped_column(Integer, default=1)
    is_manual: Mapped[bool] = mapped_column(default=False)


class AssessmentAttempt(Base):
    __tablename__ = "lms_assessment_attempt"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    assessment_id: Mapped[int] = mapped_column(ForeignKey("lms_assessment.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    responses_json: Mapped[dict] = mapped_column(JSON, default_factory=dict)
    score: Mapped[int] = mapped_column(Integer, default=0)
    status: Mapped[str] = mapped_column(String(40), default="submitted")
    graded_by: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)
    submitted_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Certificate(Base):
    __tablename__ = "lms_certificate"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    certificate_code: Mapped[str] = mapped_column(String(100), unique=True, index=True)
    issued_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class AttendanceRecord(Base):
    __tablename__ = "lms_attendance"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    session_date: Mapped[date] = mapped_column(Date)
    status: Mapped[str] = mapped_column(String(30), default="present")
    notes: Mapped[str] = mapped_column(Text, default="")
    marked_by: Mapped[Optional[int]] = mapped_column(ForeignKey("user.id"), default=None)


class Achievement(Base):
    __tablename__ = "lms_achievement"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    name: Mapped[str] = mapped_column(String(255), unique=True)
    description: Mapped[str] = mapped_column(Text, default="")
    points: Mapped[int] = mapped_column(Integer, default=0)
    criteria_json: Mapped[list[dict]] = mapped_column(JSON, default_factory=list)


class UserAchievement(Base):
    __tablename__ = "lms_user_achievement"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    achievement_id: Mapped[int] = mapped_column(ForeignKey("lms_achievement.id"), index=True)
    awarded_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Category(Base):
    __tablename__ = "lms_category"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    name: Mapped[str] = mapped_column(String(120), index=True)
    parent_id: Mapped[Optional[int]] = mapped_column(ForeignKey("lms_category.id"), default=None, index=True)


class Tag(Base):
    __tablename__ = "lms_tag"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    name: Mapped[str] = mapped_column(String(80), unique=True, index=True)


class CourseTag(Base):
    __tablename__ = "lms_course_tag"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    tag_id: Mapped[int] = mapped_column(ForeignKey("lms_tag.id"), index=True)


class Conversation(Base):
    __tablename__ = "lms_conversation"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    title: Mapped[str] = mapped_column(String(255), default="")
    is_group: Mapped[bool] = mapped_column(default=False)
    created_by: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class ConversationParticipant(Base):
    __tablename__ = "lms_conversation_participant"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None, nullable=False)
    conversation_id: Mapped[int] = mapped_column(ForeignKey("lms_conversation.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)


class Message(Base):
    __tablename__ = "lms_message"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    conversation_id: Mapped[int] = mapped_column(ForeignKey("lms_conversation.id"), index=True)
    sender_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    encrypted_content: Mapped[str] = mapped_column(Text)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class ForumTopic(Base):
    __tablename__ = "lms_forum_topic"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    title: Mapped[str] = mapped_column(String(255))
    created_by: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class ForumPost(Base):
    __tablename__ = "lms_forum_post"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    topic_id: Mapped[int] = mapped_column(ForeignKey("lms_forum_topic.id"), index=True)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    content: Mapped[str] = mapped_column(Text)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class ParentStudentLink(Base):
    __tablename__ = "lms_parent_student_link"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    parent_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    student_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class AdmissionApplication(Base):
    __tablename__ = "lms_admission_application"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    program_id: Mapped[str] = mapped_column(String(120), index=True)
    status: Mapped[str] = mapped_column(String(40), default="Pending", index=True)
    reviewer_notes: Mapped[str] = mapped_column(Text, default="")
    payload_json: Mapped[dict] = mapped_column(JSON, default_factory=dict)
    submitted_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)
    updated_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Competency(Base):
    __tablename__ = "lms_competency"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    name: Mapped[str] = mapped_column(String(255), unique=True)
    description: Mapped[str] = mapped_column(Text, default="")


class CourseCompetency(Base):
    __tablename__ = "lms_course_competency"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[int] = mapped_column(ForeignKey("lms_course.id"), index=True)
    competency_id: Mapped[int] = mapped_column(ForeignKey("lms_competency.id"), index=True)


class AssessmentCompetency(Base):
    __tablename__ = "lms_assessment_competency"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    assessment_id: Mapped[int] = mapped_column(ForeignKey("lms_assessment.id"), index=True)
    competency_id: Mapped[int] = mapped_column(ForeignKey("lms_competency.id"), index=True)


class UserCompetency(Base):
    __tablename__ = "lms_user_competency"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    competency_id: Mapped[int] = mapped_column(ForeignKey("lms_competency.id"), index=True)
    level: Mapped[int] = mapped_column(Integer, default=0)
    last_updated: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class MediaFile(Base):
    __tablename__ = "lms_media_file"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    original_name: Mapped[str] = mapped_column(String(255))
    stored_name: Mapped[str] = mapped_column(String(255), unique=True, index=True)
    content_type: Mapped[str] = mapped_column(String(120), default="application/octet-stream")
    size_bytes: Mapped[int] = mapped_column(Integer, default=0)
    storage_path: Mapped[str] = mapped_column(String(500))
    uploaded_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class Note(Base):
    __tablename__ = "lms_note"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    course_id: Mapped[Optional[int]] = mapped_column(ForeignKey("lms_course.id"), default=None, index=True)
    lesson_id: Mapped[Optional[int]] = mapped_column(ForeignKey("lms_lesson.id"), default=None, index=True)
    title: Mapped[str] = mapped_column(String(255), default="")
    content: Mapped[str] = mapped_column(Text)
    tag: Mapped[str] = mapped_column(String(80), default="")
    priority: Mapped[str] = mapped_column(String(30), default="normal")
    is_pinned: Mapped[bool] = mapped_column(default=False)
    attachment_file_id: Mapped[Optional[int]] = mapped_column(ForeignKey("lms_media_file.id"), default=None, index=True)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)
    updated_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class CalendarEvent(Base):
    __tablename__ = "lms_calendar_event"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    course_id: Mapped[Optional[int]] = mapped_column(ForeignKey("lms_course.id"), default=None, index=True)
    title: Mapped[str] = mapped_column(String(255))
    description: Mapped[str] = mapped_column(Text, default="")
    starts_at: Mapped[datetime] = mapped_column(DateTime, index=True)
    ends_at: Mapped[datetime] = mapped_column(DateTime, index=True)
    created_by: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)


class LmsNotification(Base):
    __tablename__ = "lms_notification"

    id: Mapped[Optional[int]] = mapped_column(primary_key=True, init=False, default=None)
    user_id: Mapped[int] = mapped_column(ForeignKey("user.id"), index=True)
    title: Mapped[str] = mapped_column(String(255))
    message: Mapped[str] = mapped_column(Text)
    type: Mapped[str] = mapped_column(String(80), default="info")
    is_read: Mapped[bool] = mapped_column(default=False)
    read_at: Mapped[Optional[datetime]] = mapped_column(DateTime, default=None)
    created_at: Mapped[datetime] = mapped_column(DateTime, default_factory=datetime.utcnow)
