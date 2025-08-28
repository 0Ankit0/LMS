# 13. Student Management System (SMS) Features

This document details features that are central to the Student Management System aspect of the application, focusing on user roles, profiles, and instructor tools.

## 1. Core User Roles

The system is designed around three primary user roles, which are managed using ASP.NET Core Identity Roles.

-   **`Student`**: The standard user role. Can enroll in courses, consume content, take assessments, and view their own progress, grades, and attendance.
-   **`Instructor`**: Can do everything a Student can, but also has ownership of their assigned courses. For their own courses, they can manage content, view the roster, take attendance, grade assessments, and view reports.
-   **`Admin`**: Has full system-wide permissions. Can manage all users, all courses, all settings, and view all reports.

## 2. Enhanced User Profile

To function as a proper SMS, the `User` profile must be expanded to hold more than just a name and bio. The `Users` table will include additional, optional fields for administrators and instructors to manage student information.

**Additional `User` Table Fields:**

-   `StudentIdNumber` (string): An official student ID number.
-   `EnrollmentDate` (DateTime): The date the student first enrolled in the institution.
-   `GraduationDate` (DateTime?): The student's graduation date.
-   `EmergencyContactName` (string): Name of an emergency contact.
-   `EmergencyContactPhone` (string): Phone number for an emergency contact.

*(These fields will be added to the `02-Database-Schema.md` document.)*

## 3. Enrollment Management (Course Roster)

Instructors and Admins need to be able to view and manage the list of students enrolled in a course.

**Workflow: Managing a Course Roster**

1.  **Navigation**: An instructor or admin navigates to a course dashboard and clicks the "Roster" tab.
2.  **UI**: The page displays a data grid of all currently enrolled students, populated by `GET /api/courses/{id}/enrollments`.
3.  **Manual Enrollment**: The admin/instructor clicks "Add Student". A dialog appears with a searchable dropdown of all students in the system.
4.  **API Call (Add)**: They select a student and click "Enroll". The frontend calls `POST /api/courses/{id}/enrollments` with the `userId`.
5.  **Manual Un-enrollment**: The admin/instructor clicks a "Remove" icon next to a student in the roster.
6.  **API Call (Remove)**: After a confirmation dialog, the frontend calls `DELETE /api/enrollments/{id}`.

## 4. Instructor Gradebook

The Gradebook provides a comprehensive matrix view of all student grades for all gradable items within a single course.

**UI (`Gradebook.razor`):**

-   **Layout**: A table/matrix.
    -   **Rows**: Each row represents a single student enrolled in the course.
    -   **Columns**: Each column represents a single gradable item (e.g., "Assessment 1", "Quiz: Module 2", "Final Exam"). The final column shows the student's calculated overall course grade.
-   **Data**: The entire gradebook is populated by a single, efficient call to a new endpoint: `GET /api/courses/{id}/gradebook`.
-   **Functionality**: 
    -   Cells display the score/grade for that student on that item.
    -   Clicking a cell allows the instructor to edit the grade directly (if the item is manually graded).
    -   Cells for pending manual grades are highlighted.
