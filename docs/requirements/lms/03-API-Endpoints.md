# 3. API Endpoints

This document outlines the RESTful API endpoints for the LMS. The API is built using ASP.NET Core Minimal APIs. Endpoints are grouped by resource.

## Authentication

Authentication is handled by ASP.NET Core Identity endpoints. These endpoints are crucial for managing user access throughout the student lifecycle, from initial application to course enrollment and ongoing learning.

-   `POST /register`: Registers a new user.
-   `POST /login`: Authenticates a user and returns a token.
-   `POST /logout`: Logs out the current user.
-   `POST /manage/info`: Allows users to manage their profile information.

## Courses

-   **`GET /api/courses`**: Retrieves a paginated list of courses.
    -   **Query Parameters**: `page`, `pageSize`, `search`, `category`, `level`
    -   **Response**: `PaginatedResult<CourseModel>`

-   **`GET /api/courses/{id}`**: Retrieves a single course by its ID.
    -   **Response**: `CourseModel`

-   **`POST /api/courses`**: Creates a new course. (Admin only)
    -   **Request Body**: `CreateCourseRequest`
    -   **Response**: `201 Created` with the location of the new resource.

-   **`PUT /api/courses/{id}`**: Updates an existing course. (Admin only)
    -   **Request Body**: `UpdateCourseRequest` (similar to `CreateCourseRequest`)
    -   **Response**: `204 No Content`

-   **`DELETE /api/courses/{id}`**: Deletes a course. (Admin only)
    -   **Response**: `204 No Content`

## Modules

-   **`GET /api/courses/{courseId}/modules`**: Retrieves all modules for a specific course.
-   **`POST /api/courses/{courseId}/modules`**: Creates a new module for a course. (Admin only)
-   **`PUT /api/modules/{id}`**: Updates a module. (Admin only)
-   **`DELETE /api/modules/{id}`**: Deletes a module. (Admin only)

## Lessons

-   **`GET /api/modules/{moduleId}/lessons`**: Retrieves all lessons for a specific module.
-   **`POST /api/modules/{moduleId}/lessons`**: Creates a new lesson for a module. (Admin only)
-   **`PUT /api/lessons/{id}`**: Updates a lesson. (Admin only)
-   **`DELETE /api/lessons/{id}`**: Deletes a lesson. (Admin only)

## Course Structure & Progress

-   **`GET /api/courses/{courseId}/structure`**: Retrieves the full nested structure of a course (modules and lessons) along with the current user's completion status for each item.
    -   **Response**: A `CourseStructureModel` containing a list of modules, each with a list of lessons and progress flags.

-   **`POST /api/lessons/{lessonId}/progress`**: Updates the user's progress for a specific lesson. This is called when a user completes a lesson or to save progress.
    -   **Request Body**: `{ "timeSpent": "00:15:30", "isCompleted": true }`
    -   **Response**: `200 OK` with a list of any newly unlocked achievements.

## Enrollments

-   **`GET /api/courses/{courseId}/enrollments`**: Retrieves a list of all users enrolled in a course (the course roster).
    -   **Authorization**: Admin or course Instructor.

-   **`POST /api/courses/{courseId}/enrollments`**: Manually enrolls a specific user into a course.
    -   **Authorization**: Admin or course Instructor.
    -   **Request Body**: `{ "userId": "..." }`

-   **`DELETE /api/enrollments/{enrollmentId}`**: Removes a user from a course.
    -   **Authorization**: Admin or course Instructor.

-   **`POST /api/courses/{courseId}/enroll`**: Enrolls the **current** user in a course.
    -   **Response**: `200 OK`

-   **`GET /api/my-enrollments`**: Retrieves all enrollments for the current user.
    -   **Response**: `List<EnrollmentModel>`

## Gradebook & Grading

-   **`GET /api/courses/{courseId}/gradebook`**: Retrieves the full gradebook for a course, including all students and their scores on all gradable items.
    -   **Authorization**: Admin or course Instructor.

-   **`POST /api/assessment-attempts/{id}/grade`**: Submits grades for manually graded questions within a specific assessment attempt.
    -   **Authorization**: Admin or course Instructor.

## Announcements

-   **`GET /api/announcements`**: Retrieves a list of announcements.
    -   **Query Parameters**: `courseId` (optional)
    -   **Response**: `List<AnnouncementModel>`

-   **`POST /api/announcements`**: Creates a new announcement. (Admin only)
    -   **Request Body**: `CreateAnnouncementRequest`
    -   **Response**: `201 Created`

## Assessments

-   **`GET /api/assessments/{id}`**: Retrieves an assessment's details.
    -   **Response**: `AssessmentModel`

-   **`POST /api/assessments/{id}/submit`**: Submits the current user's answers for an assessment.
    -   **Request Body**: `SubmitAssessmentRequest`
    -   **Response**: `AssessmentAttemptModel` (with score and results)

## User Management (Admin)

-   **`GET /api/certificates`**: Retrieves a list of all issued certificates. (Admin only)
-   **`GET /api/certificates/{id}`**: Retrieves a single certificate by its ID.
-   **`GET /api/users/me/certificates`**: Retrieves a list of all certificates earned by the currently authenticated user.


-   **`GET /api/admin/users`**: Retrieves a paginated list of all users.
    -   **Response**: `PaginatedResult<UserModel>`

-   **`GET /api/admin/users/{id}`**: Retrieves a single user by ID.
    -   **Response**: `UserModel`

-   **`PUT /api/admin/users/{id}`**: Updates a user's details.
    -   **Response**: `204 No Content`

-   **`DELETE /api/admin/users/{id}`**: Deletes a user.
    -   **Response**: `204 No Content`

## Parent Portal

-   **`GET /api/parent/my-children`**: Retrieves a list of students linked to the current parent user.
    -   **Authorization**: Parent
    -   **Response**: `List<StudentSummaryModel>`

-   **`GET /api/parent/children/{studentId}/dashboard`**: Retrieves a summary dashboard for a specific linked student.
    -   **Authorization**: Parent (must be linked to the `studentId`)
    -   **Response**: `StudentDashboardModel`

-   **`GET /api/parent/children/{studentId}/grades`**: Retrieves the gradebook for a specific linked student.
    -   **Authorization**: Parent (must be linked to the `studentId`)
    -   **Response**: `List<GradeModel>`

-   **`GET /api/parent/children/{studentId}/attendance`**: Retrieves the attendance records for a specific linked student.
    -   **Authorization**: Parent (must be linked to the `studentId`)
    -   **Response**: `List<AttendanceRecordModel>`

-   **`POST /api/admin/students/{studentId}/parent-links`**: Links a parent user to a student account.
    -   **Authorization**: Admin
    -   **Request Body**: `{ "parentId": "..." }`
    -   **Response**: `204 No Content`

-   **`DELETE /api/admin/students/{studentId}/parent-links/{parentId}`**: Removes the link between a parent and a student.
    -   **Authorization**: Admin
    -   **Response**: `204 No Content`

## Admissions

-   **`POST /api/admissions/apply`**: Submits a new application.
    -   **Authorization**: Authenticated User
    -   **Request Body**: `CreateApplicationRequest`
    -   **Response**: `201 Created`

-   **`GET /api/admissions/my-application`**: Retrieves the current user's application status.
    -   **Authorization**: Authenticated User
    -   **Response**: `ApplicationModel`

-   **`GET /api/admin/admissions/applications`**: Retrieves a list of all applications.
    -   **Authorization**: Admin
    -   **Query Parameters**: `status`, `programId`
    -   **Response**: `PaginatedResult<ApplicationModel>`

-   **`GET /api/admin/admissions/applications/{id}`**: Retrieves a single application.
    -   **Authorization**: Admin
    -   **Response**: `ApplicationModel` (including submitted documents)

-   **`POST /api/admin/admissions/applications/{id}/decision`**: Updates the status of an application (e.g., Accept, Reject).
    -   **Authorization**: Admin
    -   **Request Body**: `{ "status": "Accepted", "reviewerNotes": "..." }`
    -   **Response**: `204 No Content`

## Competency

-   **`GET /api/competencies`**: Retrieves a list of all defined competencies.
    -   **Authorization**: Admin, Instructor
    -   **Response**: `List<CompetencyModel>`

-   **`POST /api/competencies`**: Creates a new competency.
    -   **Authorization**: Admin
    -   **Request Body**: `CreateCompetencyRequest`
    -   **Response**: `201 Created`

-   **`POST /api/courses/{courseId}/competencies`**: Links a competency to a course.
    -   **Authorization**: Admin, Instructor
    -   **Request Body**: `{ "competencyId": ... }`
    -   **Response**: `204 No Content`

-   **`POST /api/assessments/{assessmentId}/competencies`**: Links a competency to an assessment.
    -   **Authorization**: Admin, Instructor
    -   **Request Body**: `{ "competencyId": ... }`
    -   **Response**: `204 No Content`

-   **`GET /api/users/{userId}/competency-profile`**: Retrieves the competency profile for a specific user.
    -   **Authorization**: Admin, Instructor, or the user themselves
    -   **Response**: `List<UserCompetencyModel>`

## Reports

Authorization: Access to these endpoints is restricted. An `Admin` can access any report. A non-admin user (i.e., an `Instructor`) can only access a report if they are the instructor of the course specified in the `courseId` filter.

-   **`GET /api/reports/student-engagement`**: Retrieves data for the student engagement report.
    -   **Query Parameters**: `courseId`, `startDate`, `endDate`
    -   **Response**: `ReportDto` (containing chart data)

-   **`GET /api/reports/course-completion`**: Retrieves data for the course completion report.
    -   **Query Parameters**: `courseId`, `startDate`, `endDate`
    -   **Response**: `ReportDto` (containing chart data)

## Attendance

-   **`POST /api/attendance`**: A batch endpoint for an instructor or admin to submit attendance for multiple students for a specific class session/date.
    -   **Authorization**: Admin or Instructor.

-   **`GET /api/courses/{courseId}/attendance`**: Retrieves all attendance records for a specific course.
    -   **Authorization**: Admin or course Instructor.

-   **`GET /api/users/{userId}/attendance`**: Retrieves all attendance records for a specific user (student or instructor).
    -   **Authorization**: Admin, or the user themselves.

-   **`PUT /api/attendance/{id}`**: Updates a single, existing attendance record.
    -   **Authorization**: Admin or Instructor.

## Gamification

-   **`POST /api/achievements`**: Creates a new achievement definition.
    -   **Authorization**: Admin or Instructor.

-   **`POST /api/achievements/{id}/criteria`**: Adds a rule/criterion to an existing achievement.
    -   **Authorization**: Admin or Instructor.

-   **`GET /api/leaderboard`**: Retrieves the current leaderboard rankings.
    -   **Authorization**: Public.

-   **`GET /api/users/{id}/achievements`**: Retrieves all achievements earned by a specific user.
    -   **Authorization**: Public.

## Categories & Tags

-   **`GET /api/categories`**: Retrieves the hierarchy of all course categories.
-   **`POST /api/categories`**: Creates a new category. (Admin only)
-   **`PUT /api/categories/{id}`**: Updates a category. (Admin only)
-   **`DELETE /api/categories/{id}`**: Deletes a category. (Admin only)

-   **`GET /api/tags`**: Retrieves all tags, perhaps with usage counts.
-   **`POST /api/tags`**: Creates a new tag. (Admin only)
-   **`PUT /api/tags/{id}`**: Updates a tag. (Admin only)
-   **`DELETE /api/tags/{id}`**: Deletes a tag. (Admin only)

## Communication (Forums & Messaging)

-   **`POST /api/conversations`**: Creates a new private or group conversation.
    -   **Request Body**: `{ "participantIds": ["...", "..."] }`
-   **`GET /api/conversations`**: Gets a list of all conversations for the current user.
-   **`POST /api/messages`**: Sends a new message to a conversation.
    -   **Request Body**: `{ "conversationId": ..., "encryptedContent": "..." }`
-   **`GET /api/conversations/{id}/messages`**: Gets all messages for a conversation.

-   **`POST /api/forum-topics/{id}/posts`**: Adds a new post to a forum topic.

## File Management

-   **`GET /api/my-files`**: Retrieves a list of all files in the current user's media library.
-   **`POST /api/my-files`**: Uploads a new file to the user's media library.
-   **`DELETE /api/my-files/{id}`**: Deletes a single file from the user's media library. Will return a `409 Conflict` if the file is in use.
-   **`POST /api/my-files/delete-batch`**: Deletes multiple files in a single request. Handles conflicts for in-use files.
    -   **Request Body**: `{ "fileIds": [1, 2, 3], "onConflict": "setNull" }` (onConflict is optional on first attempt).

## Notifications & Calendar

-   **`GET /api/notifications`**: Gets all notifications for the current user.
-   **`POST /api/notifications/{id}/mark-as-read`**: Marks a specific notification as read.
-   **`GET /api/calendar`**: Gets all calendar events for the current user for a given date range.
-   **`POST /api/calendar-events`**: Creates a new custom calendar event for a course.
    -   **Authorization**: Admin or course Instructor.

## Notifications & Calendar

-   **`GET /api/notifications`**: Gets all notifications for the current user.
-   **`POST /api/notifications/{id}/mark-as-read`**: Marks a specific notification as read.
-   **`GET /api/calendar`**: Gets all calendar events for the current user for a given date range.
-   **`POST /api/calendar-events`**: Creates a new custom calendar event for a course.
    -   **Authorization**: Admin or course Instructor.

## Dropdowns

-   **`GET /api/dropdown/categories`**: Retrieves a list of categories for use in dropdowns.
    -   **Response**: `List<DropdownOption>`

-   **`GET /api/dropdown/course-levels`**: Retrieves the available course levels.
    -   **Response**: `List<DropdownOption>`

-   **`GET /api/dropdown/users`**: Retrieves a list of users (e.g., for assigning instructors).
    -   **Response**: `List<DropdownOption>`

## Notes

-   **`GET /api/notes`**: Retrieves a list of notes for the current user.
    -   **Authorization**: User
    -   **Query Parameters**: `courseId`, `lessonId`, `searchTerm`, `tag`, `priority`, `isPinned`
    -   **Response**: `List<NoteModel>`

-   **`GET /api/lessons/{lessonId}/notes`**: Retrieves all notes for a specific lesson for the current user.
    -   **Authorization**: User
    -   **Response**: `List<NoteModel>`

-   **`POST /api/notes`**: Creates a new note.
    -   **Authorization**: User
    -   **Request Body**: `CreateNoteRequest`
    -   **Response**: `NoteModel`

-   **`PUT /api/notes/{id}`**: Updates an existing note.
    -   **Authorization**: User (owner of the note)
    -   **Request Body**: `UpdateNoteRequest`
    -   **Response**: `NoteModel`

-   **`DELETE /api/notes/{id}`**: Deletes a note.
    -   **Authorization**: User (owner of the note)
    -   **Response**: `204 No Content`
