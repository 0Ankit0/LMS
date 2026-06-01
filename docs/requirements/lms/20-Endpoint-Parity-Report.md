# 20. Endpoint Parity Report (Live Validation)

Generated: 2026-05-31T08:21:01.683541Z

This report is generated from live API calls executed in integration tests.

## Summary

- Total endpoint validations: 91
- Successful validations: 91
- Failed validations: 0

## Live Endpoint Matrix

| Method | Endpoint | Expected | Actual | Evidence |
|---|---|---|---|---|
| POST | /register | 200 | 200 | {"id": 4, "username": "reg_user", "email": "reg_user@test.com"} |
| POST | /login | 200 | 200 | {"access_token": "v4.local.096oKKRZ8yIqIknOMqs9EVpapn-IO3E1mN9UGQEvLViqbixghhE0hs5pDDzctFzdWL5t8od-EIKpaXHDOzaUmAjBGr... |
| POST | /manage/info | 200 | 200 | {"message": "Profile updated"} |
| POST | /logout | 200 | 200 | {"message": "Logged out"} |
| POST | /api/courses | 201 | 201 | {"id": 1, "title": "Parity Course", "description": "Course for parity", "category": "Science", "level": "Intermediate... |
| GET | /api/courses | 200 | 200 | {"items": [{"id": 1, "title": "Parity Course", "description": "Course for parity", "category": "Science", "level": "I... |
| GET | /api/courses/1 | 200 | 200 | {"id": 1, "title": "Parity Course", "description": "Course for parity", "category": "Science", "level": "Intermediate... |
| POST | /api/courses/1/modules | 201 | 201 | {"id": 1, "course_id": 1, "title": "Parity Module", "description": "mod", "order_index": 1} |
| GET | /api/courses/1/modules | 200 | 200 | [{"id": 1, "course_id": 1, "title": "Parity Module", "description": "mod", "order_index": 1}] |
| PUT | /api/modules/1 | 204 | 204 | no-content |
| POST | /api/modules/1/lessons | 201 | 201 | {"id": 1, "module_id": 1, "title": "Parity Lesson", "content": "lesson", "duration_minutes": 20} |
| GET | /api/modules/1/lessons | 200 | 200 | [{"id": 1, "module_id": 1, "title": "Parity Lesson", "content": "lesson", "duration_minutes": 20}] |
| PUT | /api/lessons/1 | 204 | 204 | no-content |
| GET | /api/courses/1/structure | 200 | 200 | {"courseId": 1, "modules": [{"id": 1, "course_id": 1, "title": "Parity Module Updated", "description": "mod2", "order... |
| POST | /api/lessons/1/progress | 200 | 200 | {"unlockedAchievements": ["lesson_completed"]} |
| POST | /api/courses/1/enrollments | 201 | 201 | {"id": 1, "course_id": 1, "user_id": 2, "role": "student", "enrolled_at": "2026-05-31T08:20:58.403885"} |
| GET | /api/courses/1/enrollments | 200 | 200 | [{"id": 1, "course_id": 1, "user_id": 2, "role": "student", "enrolled_at": "2026-05-31T08:20:58.403885"}] |
| POST | /api/courses/1/enroll | 200 | 200 | {"message": "Already enrolled", "enrollmentId": 1} |
| GET | /api/my-enrollments | 200 | 200 | [{"id": 1, "course_id": 1, "user_id": 2, "role": "student", "enrolled_at": "2026-05-31T08:20:58.403885"}] |
| GET | /api/assessments/1 | 200 | 200 | {"id": 1, "course_id": 1, "title": "Parity Quiz", "description": "quiz", "max_score": 100, "questions": [{"id": 1, "a... |
| POST | /api/assessments/1/submit | 200 | 200 | {"id": 1, "assessment_id": 1, "user_id": 2, "responses_json": {"1": "2"}, "score": 5, "status": "submitted", "graded_... |
| GET | /api/courses/1/gradebook | 200 | 200 | [{"userId": 2, "scores": [{"attemptId": 1, "assessmentId": 1, "score": 5}], "averageScore": 5.0}] |
| POST | /api/assessment-attempts/1/grade | 200 | 200 | {"id": 1, "assessment_id": 1, "user_id": 2, "responses_json": {"1": "2"}, "score": 6, "status": "graded", "graded_by"... |
| POST | /api/announcements | 201 | 201 | {"id": 1, "title": "Parity Announcement", "content": "hello", "course_id": 1, "created_by": 1, "created_at": "2026-05... |
| GET | /api/announcements | 200 | 200 | [{"id": 1, "title": "Parity Announcement", "content": "hello", "course_id": 1, "created_by": 1, "created_at": "2026-0... |
| GET | /api/certificates | 200 | 200 | [{"id": 1, "course_id": 1, "user_id": 2, "certificate_code": "PARITY-CERT-001", "issued_at": "2026-05-31T08:20:58.770... |
| GET | /api/certificates/1 | 200 | 200 | {"id": 1, "course_id": 1, "user_id": 2, "certificate_code": "PARITY-CERT-001", "issued_at": "2026-05-31T08:20:58.7703... |
| GET | /api/users/me/certificates | 200 | 200 | [{"id": 1, "course_id": 1, "user_id": 2, "certificate_code": "PARITY-CERT-001", "issued_at": "2026-05-31T08:20:58.770... |
| GET | /api/admin/users | 200 | 200 | {"items": [{"id": 4, "hashed_password": "$2b$12$L8AARduU8Sv1TxezdtokuuaLE8DA7txl7Siav.bUw2jTr7Cr/d83G", "social_provi... |
| GET | /api/admin/users/2 | 200 | 200 | {"id": 2, "hashed_password": "$2b$12$F7XR/BmhJnvFIFHO1Ym1cesjnLlIPDUYyBO9VpRijc4F4rdg38ELe", "social_provider": null,... |
| PUT | /api/admin/users/2 | 204 | 204 | no-content |
| DELETE | /api/admin/users/5 | 204 | 204 | no-content |
| POST | /api/admin/students/2/parent-links | 204 | 204 | no-content |
| GET | /api/parent/my-children | 200 | 200 | [{"id": 2, "username": "parity_student", "email": "parity_student@test.com"}] |
| GET | /api/parent/children/2/dashboard | 200 | 200 | {"studentId": 2, "enrollments": 1, "assessmentAttempts": 1} |
| GET | /api/parent/children/2/grades | 200 | 200 | [{"id": 1, "assessment_id": 1, "user_id": 2, "responses_json": {"1": "2"}, "score": 6, "status": "graded", "graded_by... |
| GET | /api/parent/children/2/attendance | 200 | 200 | [] |
| POST | /api/admissions/apply | 201 | 201 | {"id": 1, "user_id": 2, "program_id": "P-01", "status": "Pending", "reviewer_notes": "", "payload_json": {"statement"... |
| GET | /api/admissions/my-application | 200 | 200 | {"id": 1, "user_id": 2, "program_id": "P-01", "status": "Pending", "reviewer_notes": "", "payload_json": {"statement"... |
| GET | /api/admin/admissions/applications | 200 | 200 | {"items": [{"id": 1, "user_id": 2, "program_id": "P-01", "status": "Pending", "reviewer_notes": "", "payload_json": {... |
| GET | /api/admin/admissions/applications/1 | 200 | 200 | {"id": 1, "user_id": 2, "program_id": "P-01", "status": "Pending", "reviewer_notes": "", "payload_json": {"statement"... |
| POST | /api/admin/admissions/applications/1/decision | 204 | 204 | no-content |
| POST | /api/competencies | 201 | 201 | {"id": 1, "name": "Critical Thinking", "description": "Ability to reason"} |
| GET | /api/competencies | 200 | 200 | [{"id": 1, "name": "Critical Thinking", "description": "Ability to reason"}] |
| POST | /api/courses/1/competencies | 204 | 204 | no-content |
| POST | /api/assessments/1/competencies | 204 | 204 | no-content |
| GET | /api/users/2/competency-profile | 200 | 200 | [] |
| GET | /api/reports/student-engagement | 200 | 200 | {"filters": {"courseId": 1, "startDate": "2026-01-01", "endDate": "2026-12-31"}, "metrics": {"events": 1, "totalTimeS... |
| GET | /api/reports/course-completion | 200 | 200 | {"filters": {"courseId": 1, "startDate": "2026-01-01", "endDate": "2026-12-31"}, "items": [{"courseId": 1, "userId": ... |
| POST | /api/attendance | 200 | 200 | {"created": 1} |
| GET | /api/courses/1/attendance | 200 | 200 | [{"id": 1, "course_id": 1, "user_id": 2, "session_date": "2026-05-01", "status": "present", "notes": "ok", "marked_by... |
| GET | /api/users/2/attendance | 200 | 200 | [{"id": 1, "course_id": 1, "user_id": 2, "session_date": "2026-05-01", "status": "present", "notes": "ok", "marked_by... |
| PUT | /api/attendance/1 | 200 | 200 | {"id": 1, "course_id": 1, "user_id": 2, "session_date": "2026-05-01", "status": "late", "notes": "arrived late", "mar... |
| POST | /api/achievements | 201 | 201 | {"id": 1, "name": "First Steps", "description": "Completed first lesson", "points": 10, "criteria_json": []} |
| POST | /api/achievements/1/criteria | 200 | 200 | {"id": 1, "name": "First Steps", "description": "Completed first lesson", "points": 10, "criteria_json": [{"event": "... |
| GET | /api/leaderboard | 200 | 200 | [{"userId": 2, "points": 10}] |
| GET | /api/users/2/achievements | 200 | 200 | [{"id": 1, "user_id": 2, "achievement_id": 1, "awarded_at": "2026-05-31T08:21:00.168824"}] |
| POST | /api/categories | 201 | 201 | {"id": 1, "name": "Programming", "parent_id": null} |
| GET | /api/categories | 200 | 200 | [{"id": 1, "name": "Programming", "parent_id": null}] |
| PUT | /api/categories/1 | 200 | 200 | {"id": 1, "name": "Programming Updated", "parent_id": null} |
| POST | /api/tags | 201 | 201 | {"id": 1, "name": "python"} |
| GET | /api/tags | 200 | 200 | [{"id": 1, "name": "python"}] |
| PUT | /api/tags/1 | 200 | 200 | {"id": 1, "name": "python-advanced"} |
| POST | /api/conversations | 201 | 201 | {"id": 1, "title": "Direct", "is_group": false, "created_by": 2, "created_at": "2026-05-31T08:21:00.487031"} |
| GET | /api/conversations | 200 | 200 | [{"id": 1, "title": "Direct", "is_group": false, "created_by": 2, "created_at": "2026-05-31T08:21:00.487031"}] |
| POST | /api/messages | 201 | 201 | {"id": 1, "conversation_id": 1, "sender_id": 2, "encrypted_content": "ciphertext", "created_at": "2026-05-31T08:21:00... |
| GET | /api/conversations/1/messages | 200 | 200 | [{"id": 1, "conversation_id": 1, "sender_id": 2, "encrypted_content": "ciphertext", "created_at": "2026-05-31T08:21:0... |
| POST | /api/forum-topics/1/posts | 201 | 201 | {"id": 1, "topic_id": 1, "user_id": 2, "content": "Forum post", "created_at": "2026-05-31T08:21:00.670808"} |
| POST | /api/my-files | 201 | 201 | {"id": 1, "user_id": 2, "original_name": "parity.txt", "stored_name": "ef478bf18201418cb05b209ca267b0af.txt", "conten... |
| GET | /api/my-files | 200 | 200 | [{"id": 1, "user_id": 2, "original_name": "parity.txt", "stored_name": "ef478bf18201418cb05b209ca267b0af.txt", "conte... |
| POST | /api/notes | 201 | 201 | {"id": 1, "user_id": 2, "course_id": 1, "lesson_id": 1, "title": "Parity Note", "content": "Remember this", "tag": "s... |
| GET | /api/notes | 200 | 200 | [{"id": 1, "user_id": 2, "course_id": 1, "lesson_id": 1, "title": "Parity Note", "content": "Remember this", "tag": "... |
| GET | /api/lessons/1/notes | 200 | 200 | [{"id": 1, "user_id": 2, "course_id": 1, "lesson_id": 1, "title": "Parity Note", "content": "Remember this", "tag": "... |
| PUT | /api/notes/1 | 200 | 200 | {"id": 1, "user_id": 2, "course_id": 1, "lesson_id": 1, "title": "Parity Note Updated", "content": "Remember this", "... |
| DELETE | /api/my-files/1 | 409 | 409 | {"detail": "File is in use"} |
| POST | /api/my-files/delete-batch | 200 | 200 | {"deleted": [1], "conflicts": []} |
| DELETE | /api/notes/1 | 204 | 204 | no-content |
| GET | /api/notifications | 200 | 200 | [{"id": 1, "user_id": 2, "title": "Parity Notification", "message": "Test", "type": "info", "is_read": false, "read_a... |
| POST | /api/notifications/1/mark-as-read | 200 | 200 | {"id": 1, "user_id": 2, "title": "Parity Notification", "message": "Test", "type": "info", "is_read": true, "read_at"... |
| POST | /api/calendar-events | 201 | 201 | {"id": 1, "course_id": 1, "title": "Parity Event", "description": "Calendar", "starts_at": "2026-05-31T08:21:01.16437... |
| GET | /api/calendar | 200 | 200 | [{"id": 1, "course_id": 1, "title": "Parity Event", "description": "Calendar", "starts_at": "2026-05-31T08:21:01.1643... |
| GET | /api/dropdown/categories | 200 | 200 | [{"value": 1, "label": "Programming Updated"}] |
| GET | /api/dropdown/course-levels | 200 | 200 | [{"value": "Beginner", "label": "Beginner"}, {"value": "Intermediate", "label": "Intermediate"}, {"value": "Advanced"... |
| GET | /api/dropdown/users | 200 | 200 | [{"value": 1, "label": "parity_admin"}, {"value": 3, "label": "parity_parent"}, {"value": 2, "label": "parity_student... |
| DELETE | /api/enrollments/1 | 204 | 204 | no-content |
| DELETE | /api/lessons/1 | 204 | 204 | no-content |
| DELETE | /api/modules/1 | 204 | 204 | no-content |
| DELETE | /api/tags/1 | 204 | 204 | no-content |
| DELETE | /api/categories/1 | 204 | 204 | no-content |
| DELETE | /api/admin/students/2/parent-links/3 | 204 | 204 | no-content |
| DELETE | /api/courses/1 | 204 | 204 | no-content |
