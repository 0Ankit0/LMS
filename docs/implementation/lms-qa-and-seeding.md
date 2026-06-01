# LMS QA And Seeding

This runbook captures the repeatable setup, seeded accounts, seeded domain data, and manual QA checklist for the LMS implementation.

## Prerequisites

- Python and `uv` available for the backend environment
- Node.js and `npm` available for the frontend
- A reachable Postgres instance matching the backend defaults or local environment overrides
- Docker is optional. Local QA can run without Docker.

## Seed Data

Run the idempotent LMS seed script from the backend directory:

```bash
cd backend
PYTHONPATH=. uv run python scripts/seed_lms_qa.py
```

The script updates existing QA users by username, so it is safe to rerun when credentials or profile data change.

## QA Accounts

- `qa_admin` / `Admin123!` / `admin@lms-qa.example.com`
- `qa_instructor` / `Instructor123!` / `instructor@lms-qa.example.com`
- `qa_student` / `Student123!` / `student@lms-qa.example.com`
- `qa_student_two` / `Student123!` / `student2@lms-qa.example.com`
- `qa_parent` / `Parent123!` / `parent@lms-qa.example.com`

## Seeded LMS Coverage

The QA seed currently provisions enough data to exercise the documented workflows:

- course categories, tags, competencies, and competency mappings
- courses, modules, lessons, lesson progress, and enrollments
- announcements, assessments, questions, and assessment attempts
- attendance records, achievements, certificates, and leaderboard data
- conversations, participants, messages, and notifications
- notes, media files, calendar events, parent-student links, and admissions
- forum topics and posts for collaboration-related endpoints

## Local Startup

Backend API:

```bash
cd backend
PYTHONPATH=. uv run uvicorn src.main:app --host 0.0.0.0 --port 8000
```

Frontend preview:

```bash
cd frontend
npm install
npm run build
npm run preview -- --host 0.0.0.0 --port 4173
```

If port `4173` is already occupied, Vite will move to the next free port. Use the printed preview URL for route checks.

## Recommended Verification Commands

Frontend static validation:

```bash
cd frontend
npm run typecheck
npm run build
```

Backend/API smoke validation:

```bash
cd backend
PYTHONPATH=. uv run python - <<'PY'
import requests

base = 'http://localhost:8000'
session = requests.Session()
login = session.post(
    f'{base}/api/v1/auth/login/?set_cookie=false',
    json={'username': 'qa_admin', 'password': 'Admin123!'},
)
login.raise_for_status()
session.headers['Authorization'] = f"Bearer {login.json()['access']}"

checks = [
    ('users', '/api/v1/users/?skip=0&limit=12', None),
    ('engagement', '/api/reports/student-engagement', None),
    ('completion', '/api/reports/course-completion', None),
    ('logs', '/observability/logs', {'limit': 5}),
    ('logs-summary', '/observability/logs/summary', None),
    (
        'calendar',
        '/api/calendar',
        {'startDate': '2026-06-01T00:00:00Z', 'endDate': '2026-07-31T23:59:59Z'},
    ),
]

for name, path, params in checks:
    response = session.get(f'{base}{path}', params=params)
    print(name, response.status_code)
PY
```

Expected result: each check returns `200`.

## Manual QA Checklist

Use the frontend preview URL and sign in as `qa_admin` unless a different role is called out.

### Public/Auth

- `/` loads the LMS-branded landing page
- `/login` loads with LMS branding and authenticates successfully
- `/signup` loads without template branding regressions

### Learner Routes

- `/dashboard`
- `/course-catalog`
- `/my-courses`
- `/my-courses/:courseId`
- `/announcements`
- `/assessments`
- `/messages`
- `/leaderboard`
- `/achievements`
- `/certificates`
- `/notes`
- `/calendar`
- `/files`
- `/parent-dashboard`
- `/profile`
- `/notifications`
- `/settings`

Verify that each page loads without a blank screen, unhandled request failures, or console-visible runtime crashes. On `/calendar`, confirm the empty state or event list renders instead of a `422` failure.

Specific learner checks:

- `/dashboard` changes its lead section based on the signed-in LMS role and shows student, instructor, or parent-specific cards when applicable
- `/my-courses/:courseId` renders live module and lesson data, supports progress updates, and recalculates course completion
- the course workspace assessment panel renders teacher-authored questions, accepts responses, and appends a new attempt to history after submission
- when signed in as `qa_instructor`, `/my-courses/:courseId` also renders the instructor gradebook panel and can mark submitted attempts as graded
- when signed in as `qa_parent`, the sidebar only exposes the parent dashboard plus shared account pages, and direct visits to learner-only routes such as `/my-courses` or `/assessments` redirect back to `/dashboard`

### Admin Routes

- `/admin/dashboard`
- `/admin/users`
- `/admin/courses`
- `/admin/admissions`
- `/admin/reports`
- `/admin/competencies`
- `/admin/attendance`
- `/admin/taxonomy`
- `/admin/rbac`
- `/admin/security-review`
- `/admin/logs`

Specific checks:

- `/admin/users` loads the paginated list without a backend `500`
- `/admin/reports` renders engagement metrics and completion rows without a client runtime error
- `/admin/courses` can create and delete a course against seeded taxonomy data
- `/admin/logs` renders the live observability feed, respects `logId` deep links from security review, and loads summary counters without redirecting away from the route

## Current Baseline

The latest QA pass validated these fixes:

- calendar requests now send a date range and no longer return `422`
- admin reports now match the live API response shape
- IAM user listing no longer fails on seeded QA accounts with reserved-domain emails
- frontend LMS branding is applied on the sidebar and auth shell
- student course workspaces now expose lesson progress tracking and assessment submission
- instructor course workspaces now expose the course gradebook and grading actions for submitted attempts
- instructor dashboards now load course summaries from LMS instructor assignments
- parent dashboards now show linked-student identity, grade attempts, and attendance summaries
- parent users are now redirected away from learner-only routes instead of landing on misleading empty-state course pages
- the admin navigation and quick actions now land on a real `/admin/logs` page backed by the observability APIs

## Local Constraints Noted During QA

- Docker was unavailable in the local environment, so services were run directly
- Alembic was blocked by a stale revision in local state, so the seed path relied on database initialization already present in the app runtime
- A trapped `bcrypt` version warning may appear during seeding, but the seed completes successfully