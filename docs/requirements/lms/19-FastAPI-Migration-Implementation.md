# 19. FastAPI Migration Implementation

## Overview

The LMS solution has been migrated from .NET to a FastAPI-first architecture based on the `fastapi_template` baseline.

- Legacy .NET solution/projects were removed from `main`.
- The backend now runs from `backend/src/main.py` using template infrastructure.
- LMS domain functionality is implemented in `backend/src/apps/lms/`.

## Implemented LMS Module

- Models: `backend/src/apps/lms/models.py`
- API endpoints: `backend/src/apps/lms/api.py`
- Router registration: `backend/src/main.py`
- DB model initialization: `backend/src/db/session.py`

## Endpoint Coverage

The following documented endpoint groups are implemented under documented paths:

- Authentication compatibility: `/register`, `/login`, `/logout`, `/manage/info`
- Courses: `/api/courses` CRUD
- Modules: `/api/courses/{courseId}/modules`, `/api/modules/{id}`
- Lessons: `/api/modules/{moduleId}/lessons`, `/api/lessons/{id}`
- Course structure and progress: `/api/courses/{courseId}/structure`, `/api/lessons/{lessonId}/progress`
- Enrollments: `/api/courses/{courseId}/enrollments`, `/api/enrollments/{enrollmentId}`, `/api/courses/{courseId}/enroll`, `/api/my-enrollments`
- Gradebook and grading: `/api/courses/{courseId}/gradebook`, `/api/assessment-attempts/{id}/grade`
- Announcements: `/api/announcements`
- Assessments: `/api/assessments/{id}`, `/api/assessments/{id}/submit`
- Certificates: `/api/certificates`, `/api/certificates/{id}`, `/api/users/me/certificates`
- Admin user management: `/api/admin/users` CRUD
- Parent portal: `/api/parent/*`, admin link management
- Admissions: `/api/admissions/*`, `/api/admin/admissions/*`
- Competencies: `/api/competencies`, course/assessment links, user profile
- Reports: `/api/reports/student-engagement`, `/api/reports/course-completion`
- Attendance: `/api/attendance`, `/api/courses/{courseId}/attendance`, `/api/users/{userId}/attendance`
- Gamification: `/api/achievements`, `/api/achievements/{id}/criteria`, `/api/leaderboard`, `/api/users/{id}/achievements`
- Categories and tags: `/api/categories` CRUD, `/api/tags` CRUD
- Communication: `/api/conversations`, `/api/messages`, `/api/conversations/{id}/messages`, `/api/forum-topics/{id}/posts`
- File management: `/api/my-files`, `/api/my-files/{id}`, `/api/my-files/delete-batch`
- Notifications and calendar: `/api/notifications`, `/api/notifications/{id}/mark-as-read`, `/api/calendar`, `/api/calendar-events`
- Dropdowns: `/api/dropdown/categories`, `/api/dropdown/course-levels`, `/api/dropdown/users`
- Notes: `/api/notes`, `/api/lessons/{lessonId}/notes`, note create/update/delete

## Notes

- Auth/authorization checks are enforced using token-based user resolution and role/superuser checks.
- Models are intentionally normalized around required LMS operations from docs.
- Existing template modules (IAM, notifications, analytics, etc.) remain available.
- Endpoint behavior is covered by integration tests in `backend/tests/test_lms_endpoints.py`.
