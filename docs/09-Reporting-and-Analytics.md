# 9. Reporting and Analytics

This document details the data collection strategy and the specific reports required to monitor the health and effectiveness of the LMS.

## 1. Data Collection Strategy

To generate meaningful reports, the system must diligently record all significant events. The primary mechanism for this is to ensure that every user action or system event creates or updates a record in the database.

**Key Data to Record:**

-   **User Events**: All logins, logouts, and registrations must be captured. The existing `UserActivity` table is designed for this. The `ActivityType` enum should be expanded to include more events if needed.
-   **Course Enrollment**: Every time a user enrolls in a course, a new record in the `Enrollments` table must be created. This is the foundation for most course-related reporting.
-   **Progress Tracking**: The `LessonProgress` and `ModuleProgress` tables are critical. Every lesson completion, including the `TimeSpent`, must be recorded.
-   **Assessment Attempts**: Every single attempt on an assessment must be stored in the `AssessmentAttempts` table. This includes the score, pass/fail status, and `TimeTaken`.
-   **Individual Answers**: For detailed analysis, every response to every question within an attempt should be saved in the `QuestionResponses` table.

**Data Aggregation Strategy:**

As mentioned in the Integration Workflows, generating reports directly from these transactional tables can be slow. The recommended approach is to use a scheduled background job (e.g., a nightly service) to pre-aggregate the data into dedicated summary tables (e.g., `CourseDailyStats`, `UserMonthlyActivity`). The API endpoints that feed the reports should query these summary tables for fast responses.

## 2. Administrator & Instructor Reports

This section defines the reports available in the `/reports` section of the application. Access is role-based: Administrators can view these reports for any course, while Instructors can only view reports for courses they are assigned to.

### Report 1: User Activity

-   **Description**: Provides an overview of user engagement with the platform itself.
-   **Key Metrics & Visualizations**:
    -   Line chart of Daily/Weekly/Monthly Active Users.
    -   Bar chart of new user registrations by week/month.
    -   Table of most recent user registrations.
    -   KPI cards showing: Total Users, Currently Active Users.
-   **Data Sources**: `UserActivity`, `Users` tables.
-   **Filters**: Date Range.

### Report 2: Enrollment Trends

-   **Description**: Shows how users are enrolling in courses over time.
-   **Key Metrics & Visualizations**:
    -   Line chart of total course enrollments over time.
    -   Pie chart of enrollments by course category.
    -   Data table of the most popular courses (by enrollment number) in a given period.
-   **Data Sources**: `Enrollments`, `Courses`, `Categories` tables.
-   **Filters**: Date Range, Course Category.

### Report 3: Course Completion

-   **Description**: Provides a detailed breakdown of user progress and completion rates for a specific course.
-   **Key Metrics & Visualizations**:
    -   Funnel chart showing user drop-off from one module to the next.
    -   Bar chart showing the completion percentage for each lesson in the course.
    -   KPI cards for the selected course: Completion Rate (%), Average Time to Complete, Number of Certificates Issued.
    -   Data table of all enrolled users with their individual progress percentage and completion date.
-   **Data Sources**: `Enrollments`, `ModuleProgress`, `LessonProgress` tables.
-   **Filters**: Course (required), Date Range (for enrollments).

### Report 4: Assessment Performance

-   **Description**: Analyzes user performance on a specific quiz or exam.
-   **Key Metrics & Visualizations**:
    -   Histogram of score distribution (e.g., how many users scored 0-10%, 11-20%, etc.).
    -   KPI cards for the selected assessment: Average Score, Pass/Fail Rate, Average Number of Attempts.
    -   Data table breaking down each question, showing the percentage of users who answered it correctly.
-   **Data Sources**: `AssessmentAttempts`, `QuestionResponses`, `Questions` tables.
-   **Filters**: Assessment (required).

### Report 5: Instructor Performance

-   **Description**: Compares key performance indicators across different instructors.
-   **Key Metrics & Visualizations**:
    -   Data table listing all instructors.
    -   For each instructor, columns for: Total Assigned Courses, Total Student Enrollments, Average Course Completion Rate across all their courses.
-   **Data Sources**: `Users` (filtered by instructor role), `Courses`, `Enrollments`.
-   **Filters**: Date Range.

### Report 6: Attendance Report

-   **Description**: Provides a detailed and summary view of attendance records.
-   **Key Metrics & Visualizations**:
    -   Data table showing attendance records, color-coded by status.
    -   KPI cards for: Overall Attendance Rate, Total Absences, Total Lates for the filtered scope.
-   **Data Sources**: `Attendance`, `Users`, `Courses`.
-   **Filters**: Course, Student, Date Range.

## 3. User-Facing Report (Transcript)

While not an admin report, this is a critical reporting feature for the end-user.

-   **Name**: My Transcript / Learning History
-   **Location**: User Profile page (`/user/profile`)
-   **Description**: Shows a single user their complete history on the platform.
-   **Content**:
    -   A list of all enrolled courses.
    -   For each course: Status (In Progress, Completed), Completion Date, Final Grade/Score.
    -   A list of all earned achievements with the date they were awarded.
    -   A list of all earned certificates with links to download them.
-   **Data Sources**: `Enrollments`, `UserAchievements`, `Certificates`.
