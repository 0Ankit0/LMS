# 4. User Workflows

This document describes common user workflows and interactions within the LMS.

## 1. New User Registration and First Login

**Actor**: A new, anonymous user.

**Goal**: To create an account and log in to the system.

1.  **Navigation**: The user navigates to the LMS homepage.
2.  **Action**: The user clicks the "Register" button.
3.  **Form Fill**: The user is presented with a registration form (`/register`) and enters their details (First Name, Last Name, Email, Password).
4.  **Submission**: The user submits the form. The frontend calls `POST /register`.
5.  **Confirmation**: The system creates the user account and sends a confirmation email (if configured).
6.  **Login**: The user is redirected to the login page (`/login`). They enter their credentials.
7.  **Authentication**: The frontend calls `POST /login`. Upon success, the user is authenticated.
8.  **Redirection**: The user is redirected to their personal dashboard or the homepage, now seeing authenticated options like "My Courses".

## 2. Course Discovery and Enrollment

**Actor**: An authenticated user.

**Goal**: To find and enroll in a course.

1.  **Navigation**: The user navigates to the homepage or the "All Courses" page.
2.  **Search/Filter**: The user utilizes the search bar and filter options (category, level) to find interesting courses. The UI calls `GET /api/courses` with the appropriate query parameters.
3.  **View Course**: The user clicks on a course to view its details. The application navigates to the course detail page (`/course/{id}`).
4.  **Review Details**: The user reviews the course description, modules, lessons, and instructor information, fetched via `GET /api/courses/{id}`.
5.  **Enroll**: The user clicks the "Enroll Now" button.
6.  **API Call**: The application calls `POST /api/courses/{courseId}/enroll`.
7.  **Confirmation**: The UI updates to show that the user is enrolled. The button might change to "Go to Course".
8.  **Access Course**: The user can now access the course content and it will appear on their "My Courses" page.

## 3. Learning Workflow: Completing a Course

**Actor**: An enrolled, authenticated user.

**Goal**: To progress through and complete a course.

1.  **Start Course**: The user navigates to their dashboard and clicks on an enrolled course.
2.  **Course Player**: The user is taken to the course player interface. The left side shows the course structure (modules and lessons), and the main area displays the current lesson's content.
3.  **Complete Lesson**: After watching a video or reading text, the user clicks "Mark as Complete".
4.  **Progress Update**: The application calls an API endpoint (e.g., `POST /api/lessons/{id}/complete`) to record the progress. The UI updates to show a checkmark next to the completed lesson.
5.  **Navigate**: The user clicks the "Next Lesson" button or selects another lesson from the navigation.
6.  **Take Assessment**: When the user reaches an assessment, they click "Start Quiz".
7.  **Submit Assessment**: After answering the questions, the user submits the assessment. The application calls `POST /api/assessments/{id}/submit`.
8.  **View Results**: The user immediately sees their score and which questions they got right or wrong (if configured).
9.  **Course Completion**: Once all required lessons and assessments are completed, the system marks the course as complete for that user (`CompletedAt` is set in the `Enrollments` table).
10. **Certificate**: If applicable, a "Download Certificate" button appears, allowing the user to download their certificate of completion.

## 4. Administrator: Course Management

**Actor**: An Administrator.

**Goal**: To create, update, and manage courses.

1.  **Navigation**: The admin navigates to the Admin section and then to "Course Management" (`/admin/courses`).
2.  **View Courses**: The page displays a table of all courses, fetched via `GET /api/courses`.
3.  **Create Course**: The admin clicks "Create New Course".
4.  **Form Fill**: The admin fills out the course creation form (title, description, instructor, category, etc.).
5.  **Submission**: The admin submits the form, which calls `POST /api/courses`.
6.  **Edit Course**: To edit, the admin clicks an "Edit" button in the course table. This loads the course data into the form.
7.  **Update**: The admin modifies the data and submits, calling `PUT /api/courses/{id}`.
8.  **Manage Content**: Within the course edit page, the admin can add/edit/delete modules and lessons, which triggers calls to the respective API endpoints (`/api/modules`, `/api/lessons`).

## 5. Administrator: Viewing Reports

**Actor**: An Administrator.

**Goal**: To view graphical reports on system usage.

1.  **Navigation**: The admin navigates to the Reports section (`/reports`).
2.  **Select Report**: The admin chooses a report to view, for example, "Student Engagement".
3.  **API Call**: The page loads and calls the relevant API endpoint, e.g., `GET /api/reports/student-engagement`.
4.  **View Chart**: The data returned from the API is used to render charts and diagrams (e.g., using a library like Chart.js or a Blazor-specific charting component).
5.  **Filter Data**: The admin can use filters (e.g., by date range or course) to refine the report. Each change in a filter triggers a new API call to fetch the updated data.
