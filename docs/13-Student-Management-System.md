# 13. Student Management System (SMS) Features

This document details features that are central to the Student Management System aspect of the application, focusing on user roles, profiles, and instructor tools, all contributing to the comprehensive student lifecycle management.

## 1. Core User Roles

The system is designed around three primary user roles, which are managed using ASP.NET Core Identity Roles.

-   **`Student`**: The standard user role. Can enroll in courses, consume content, take assessments, and view their own progress, grades, and attendance.
-   **`Instructor`**: Can do everything a Student can, but also has ownership of their assigned courses. For their own courses, they can manage content, view the roster, take attendance, grade assessments, and view reports.
-   **`Admin`**: Has full system-wide permissions. Can manage all users, all courses, all settings, and view all reports.
-   **`Parent`**: A special role for parents or guardians. Can view the progress, grades, attendance, and calendar for their linked children, but cannot interact with course content directly.

## 2. Parent/Guardian Portal

To support parents and guardians, the system will provide a dedicated portal. This requires linking parent accounts to student accounts.

**Data Model: `ParentStudentLinks`**

A new junction table is required to create the many-to-many relationship between parent users and student users.

| Field | Type | Description |
| --- | --- | --- |
| `ParentId` | `string` | Foreign key to the `Users` table (for the parent). |
| `StudentId` | `string` | Foreign key to the `Users` table (for the student). |

**Admin Workflow: Linking a Parent to a Student**

1.  **Navigation**: An administrator navigates to a student's profile page in the admin dashboard.
2.  **Action**: The admin clicks on a "Manage Parent/Guardian Links" tab or button.
3.  **UI**: The interface shows a list of currently linked parents and provides a searchable dropdown to find and add another user as a parent.
4.  **API Call**: The admin selects a user and clicks "Add Link". The frontend calls an endpoint like `POST /api/admin/students/{studentId}/parent-links` with the `parentId`.

**Parent Workflow: Viewing a Child's Dashboard**

1.  **Navigation**: A user with the `Parent` role logs in.
2.  **UI**: Their main dashboard displays a list of their linked children.
3.  **Action**: The parent clicks on a child's name.
4.  **Dashboard View**: They are taken to a read-only "student dashboard" view for that child, which includes:
    -   A summary of enrolled courses and overall progress.
    -   A view of the student's gradebook.
    -   A view of the student's attendance records.
    -   A combined calendar showing the student's deadlines and events.

## 3. Enhanced User Profile

To function as a proper SMS, the `User` profile must be expanded to hold more than just a name and bio. The `Users` table will include additional, optional fields for administrators and instructors to manage student information.

**Additional `User` Table Fields:**

-   `StudentIdNumber` (string): An official student ID number.
-   `EnrollmentDate` (DateTime): The date the student first enrolled in the institution.
-   `GraduationDate` (DateTime?): The student's graduation date.
-   `EmergencyContactName` (string): Name of an emergency contact.
-   `EmergencyContactPhone` (string): Phone number for an emergency contact.

*(These fields will be added to the `02-Database-Schema.md` document.)*

## 4. Enrollment Management (Course Roster)

Instructors and Admins need to be able to view and manage the list of students enrolled in a course.

**Workflow: Managing a Course Roster**

1.  **Navigation**: An instructor or admin navigates to a course dashboard and clicks the "Roster" tab.
2.  **UI**: The page displays a data grid of all currently enrolled students, populated by `GET /api/courses/{id}/enrollments`.
3.  **Manual Enrollment**: The admin/instructor clicks "Add Student". A dialog appears with a searchable dropdown of all students in the system.
4.  **API Call (Add)**: They select a student and click "Enroll". The frontend calls `POST /api/courses/{id}/enrollments` with the `userId`.
5.  **Manual Un-enrollment**: The admin/instructor clicks a "Remove" icon next to a student in the roster.
6.  **API Call (Remove)**: After a confirmation dialog, the frontend calls `DELETE /api/enrollments/{id}`.

## 5. Instructor Gradebook

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
