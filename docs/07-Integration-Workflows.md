# 7. Detailed Integration Workflows

This document provides a detailed, step-by-step breakdown of how different parts of the system interact to deliver key features. It expands on the general workflows with specific implementation logic.

## 1. Workflow: Loading and Displaying a Structured Course

**Goal**: A user opens a course they are enrolled in and sees the complete structure (modules, lessons) with their current progress clearly marked.

1.  **Trigger**: The user navigates to the Course Player page (e.g., `/user/learn/{courseId}`).

2.  **Frontend (Blazor Page)**:
    -   The page's `OnInitializedAsync` lifecycle method is triggered.
    -   It makes an HTTP request to the new endpoint: `GET /api/courses/{courseId}/structure`.

3.  **Backend (API Endpoint)**:
    -   The endpoint receives the request.
    -   It queries the database to get the course, its modules, and all its lessons, ordered correctly.
    -   It then fetches the specific user's progress data by querying `ModuleProgress` and `LessonProgress` tables, linked to the user's `EnrollmentId` for that course.
    -   The backend constructs a nested DTO (`CourseStructureModel`). This model contains a list of `Module` objects, and each `Module` object contains a list of `Lesson` objects. Crucially, each lesson and module object in the DTO has an `IsCompleted` flag.
    -   The endpoint serializes this DTO to JSON and returns it in the response.

4.  **Frontend (Blazor Page)**:
    -   The page receives the `CourseStructureModel` data.
    -   It uses this data to render the course navigation panel. It iterates through the modules and their lessons.
    -   For each lesson, it checks the `IsCompleted` flag. If `true`, it displays a checkmark icon (e.g., `Icons.Material.Filled.CheckCircle`) next to the lesson title.
    -   The first uncompleted lesson is automatically loaded into the main content area.

## 2. Workflow: Progress Tracking and Automatic Achievement Granting

**Goal**: When a user completes a lesson, their progress is saved, and the system automatically checks if they have earned any achievements.

1.  **Trigger**: The user is on a lesson page inside the Course Player and clicks the "Mark as Complete" button. (Note: This same workflow applies to completing a module, but is triggered by the completion of the final lesson within it).

2.  **Frontend (Blazor Component)**:
    -   An `@onclick` event is triggered.
    -   The handler method makes an HTTP request: `POST /api/lessons/{lessonId}/progress`.
    -   The request body includes `{ "isCompleted": true }`.

3.  **Backend (API Endpoint & Services)**:
    -   The `.../progress` endpoint receives the request.
    -   **Step 3a (Pre-condition Check)**: The `ProgressService` is called. Before any updates, it first checks the parent module (or the lesson itself) for a completion gate.
        -   It checks if the `Module.MustPassGatingAssessment` flag is `true`.
        -   If `true`, it queries the `AssessmentAttempts` table to see if there is at least one record for the current user and the `Module.GatingAssessmentId` where `IsPassed = true`.
        -   **If the check fails**: The service immediately returns an error. The API responds with a `400 Bad Request` and a message like, "You must successfully pass the 'Final Exam' before completing this module."
    -   **Step 3b (Progress Service)**: If the pre-condition check passes (or if there is no gate), the service proceeds with updating the user's progress.
        -   It finds or creates a `LessonProgress` record and marks it as complete.
        -   It recalculates the parent `ModuleProgress` percentage.
        -   It recalculates the overall `Enrollment` `ProgressPercentage` for the course.
    -   **Step 3c (Gamification & Certificate Services)**: After the progress update is successfully saved, the endpoint then triggers the `GamificationService` (as detailed previously) and, if the course is now complete, the `CertificateService` (as detailed previously).
    -   **Step 3d (Response)**: The endpoint gathers a list of any newly granted achievements and returns them in the API response.

4.  **Frontend (Blazor Page)**:
    -   The API call returns a `200 OK` response, with the list of new achievements in the body.
    -   The page checks if the response body contains any achievements.
    -   For each new achievement, it displays a temporary notification (a "toast" or "snackbar") on the screen: `"Achievement Unlocked: First Steps!"`.
    -   The navigation panel is refreshed (or the specific lesson item is updated) to show the checkmark for the just-completed lesson.

## 3. Workflow: Report Data Aggregation

**Goal**: An administrator views a report with up-to-date, aggregated data.

1.  **Trigger**: An admin navigates to a report page, e.g., `/reports/course-completion`.

2.  **Frontend (Blazor Page)**:
    -   The page loads and calls the relevant API endpoint, e.g., `GET /api/reports/course-completion`.
    -   The request can include query parameters to filter the data, such as `?timespan=last30days`.

3.  **Backend (API Endpoint & `ReportService`)**:
    -   The API endpoint receives the request and calls a dedicated `ReportService` to handle the business logic.
    -   **Logic Option A (On-the-Fly Aggregation)**:
        -   For a specific report like "Course Completion Rate", the service queries the `Enrollments` table.
        -   It calculates the total number of enrollments and the number of completed enrollments (`CompletedAt != null`) within the given timespan.
        -   It returns a DTO with the calculated data, e.g., `{ "labels": ["Completed", "In Progress"], "data": [75, 25] }`.
    -   **Logic Option B (Scheduled Aggregation - More Performant)**:
        -   A scheduled background job (e.g., a nightly `IHostedService`) runs independently of user requests.
        -   This job performs the heavy calculations, processing raw data from tables like `Enrollments` and `UserActivity`.
        -   It saves the aggregated results into new, denormalized tables (e.g., `DailyStats`, `MonthlyCourseCompletions`).
        -   When the admin requests a report, the `ReportService` simply queries these pre-aggregated summary tables, which is significantly faster.
    -   The endpoint returns the DTO provided by the service.

4.  **Frontend (Blazor Page)**:
    -   The page receives the report DTO.
    -   This data is passed to a reusable chart component (`Chart.razor`), which renders the data as a visual graph (e.g., a pie chart or bar chart).

## 4. Workflow: Automatic Certificate Generation and Delivery

**Goal**: A user who completes a course automatically receives a verifiable certificate.

1.  **Trigger**: This workflow is initiated from another process. Specifically, after the `ProgressService` updates a user's course progress and their `Enrollment` `ProgressPercentage` reaches 100, it triggers this new certificate workflow.

2.  **Backend (`CertificateService`)**:
    -   A `CertificateService` is called with the user and course information.
    -   **Step 2a (Create Record)**: The service first checks if a certificate already exists for this user and course to prevent duplicates. If not, it creates a new record in the `Certificates` database table. It generates a unique `CertificateNumber` (e.g., a GUID) and saves the `UserId`, `CourseId`, `FinalGrade`, and `IssuedAt` timestamp.
    -   **Step 2b (Generate PDF)**: The service uses a PDF generation library (e.g., QuestPDF).
        -   It loads a predefined certificate template (e.g., an HTML or Fluent C# template).
        -   It populates the template with the user's full name, the course title, the issue date, and the unique certificate number.
    -   **Step 2c (Store PDF)**: The generated PDF file is saved to a designated storage location (e.g., `wwwroot/uploads/certificates/` or a cloud blob storage container). The public-facing URL to this file is saved in the `CertificateUrl` field of the new `Certificates` record.
    -   **Step 2d (Notify User)**: The service calls an `EmailService` to send a notification to the user. The email congratulates them and provides a link to their profile page where they can view and download their new certificate.

3.  **Frontend (User Experience)**:
    -   The user receives the congratulatory email.
    -   Later, when the user navigates to their profile page (`/user/profile`), the page calls `GET /api/users/me/certificates`.
    -   The API returns a list of all their earned certificates.
    -   The UI displays a "My Certificates" section, listing each certificate and providing a "Download" button that links directly to the `CertificateUrl` for the PDF file.
