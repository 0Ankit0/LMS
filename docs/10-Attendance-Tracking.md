# 10. Attendance Tracking

This document outlines the feature for tracking student and instructor attendance.

## 1. Overview & Data Model

The attendance feature allows instructors and administrators to log and monitor presence for courses, particularly those with live or scheduled sessions. The system is designed to track both student attendance and instructor presence.

The core data is stored in the `Attendance` table:

| Field | Type | Description |
| --- | --- | --- |
| `Id` | `int` | Primary Key. |
| `ClassId` | `int` | Foreign key to the `Courses` table, representing the class session. |
| `StudentId` | `string?` | Foreign key to the `Users` table. Used when tracking a student's attendance. Null if tracking an instructor. |
| `InstructorId` | `string?` | Foreign key to the `Users` table. Used when tracking an instructor's presence. Null if tracking a student. |
| `Date` | `DateTime` | The specific date of the attendance record. |
| `Status` | `AttendanceStatus` | The status, from an enum (e.g., Present, Absent, Late, Excused). |
| `CheckInTime` | `TimeSpan?` | The time the user checked in. |
| `CheckOutTime` | `TimeSpan?` | The time the user checked out. |
| `Notes` | `string?` | Optional notes from the person taking attendance. |

## 2. API Endpoints

-   **`POST /api/attendance`**: A batch endpoint for an instructor or admin to submit attendance for multiple students for a specific class session/date.
    -   **Authorization**: Admin or Instructor.
    -   **Request Body**: A list of attendance records: `[{ "studentId": "...", "status": "Present", "notes": "..." }]`.

-   **`GET /api/courses/{courseId}/attendance`**: Retrieves all attendance records for a specific course.
    -   **Authorization**: Admin or course Instructor.

-   **`GET /api/users/{userId}/attendance`**: Retrieves all attendance records for a specific user (student or instructor).
    -   **Authorization**: Admin, or the user themselves.

-   **`PUT /api/attendance/{id}`**: Updates a single, existing attendance record.
    -   **Authorization**: Admin or Instructor.

## 3. Workflows

### Workflow 1: Instructor Takes Attendance

1.  **Navigation**: An instructor navigates to their course dashboard and selects an "Attendance" tab.
2.  **UI**: The UI displays a calendar or a list of dates for the scheduled class sessions. The instructor selects a date.
3.  **Action**: The system displays a list of all students enrolled in the course. The default status for all students is "Present".
4.  **Data Entry**: The instructor changes the status for any students who are `Absent`, `Late`, or `Excused`. They can add notes for specific students.
5.  **API Call**: The instructor clicks "Submit Attendance". The frontend bundles all student records into a list and calls `POST /api/attendance`.

### Workflow 2: Admin Views Attendance Report

1.  **Navigation**: An admin navigates to the Reports section and selects the "Attendance Report".
2.  **UI**: The report page provides filters for `Course`, `Student`, and `Date Range`.
3.  **API Call**: After setting filters, the admin clicks "Run Report". The frontend calls the relevant endpoint (e.g., `GET /api/courses/{courseId}/attendance`).
4.  **Display**: The data is displayed in a table. Statuses are color-coded for easy scanning. The report includes summary statistics like "Overall Attendance Rate" for the selected scope.

### Workflow 3: Student Views Their Own Attendance

1.  **Navigation**: A student navigates to a specific course they are enrolled in.
2.  **UI**: They select a "My Attendance" tab.
3.  **API Call**: The frontend calls `GET /api/users/{self_id}/attendance?courseId={courseId}` to retrieve only their own attendance for that course.
4.  **Display**: The UI shows a simple list or calendar view of their attendance history, including their status for each recorded day.
