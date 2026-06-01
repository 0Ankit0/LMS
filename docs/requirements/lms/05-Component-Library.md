# 5. Component & UI Structure

This document outlines the UI structure of the LMS, including layouts, page organization, and key Blazor components.

## 1. Main Layout

The main user interface is built around a primary layout component.

-   **`MainLayout.razor`**: This is the core layout for the entire application. It defines the overall page structure.
    -   **Top Navigation (`TopNav.razor`)**: A persistent navigation bar at the top of the page. It contains the application logo, links to key areas, and the user profile/login/register buttons.
    -   **Side Navigation (Optional)**: For specific sections like the Admin dashboard or the Course Player, a sidebar may be used for secondary navigation.
    -   **Content Area (`@Body`)**: The main area where the content of individual pages is rendered.
    -   **Footer**: A consistent footer at the bottom of every page.

## 2. Page Organization

The application's pages are organized into three main folders as requested, reflecting the primary user roles and tasks.

### `/Components/Pages/`

#### General Pages

-   **`Home.razor` (`/`)**: The main landing page. As specified, it should be designed with three columns:
    1.  **Quick Navigation**: Links for authenticated users (My Courses, Profile) or calls to action for anonymous users (Register, View Courses).
    2.  **Announcements**: A feed of recent announcements, fetched from `GET /api/announcements`.
    3.  **Course Search**: A prominent search bar and filters to discover courses.
-   **`Courses.razor` (`/courses`)**: A page to display all available courses with advanced search and filtering. The filtering UI should include options for categories and tags.
-   **`CourseDetail.razor` (`/course/{id}`)**: The public-facing detail page for a single course.
-   **`Announcements.razor` (`/announcements`)**: A page dedicated to viewing all announcements.
-   **`Leaderboard.razor` (`/leaderboard`)**: A public page displaying the top users by points.
-   **`Calendar.razor` (`/calendar`)**: A page displaying a calendar with all of a user's deadlines and events.

#### User Section (`/Components/Pages/User/`)

-   **`Dashboard.razor` (`/user/dashboard`)**: The user's personal dashboard, showing their enrolled courses, progress, and recent achievements.
-   **`MyCourses.razor` (`/user/my-courses`)**: A list of all courses the user is enrolled in.
-   **`CoursePlayer.razor` (`/user/learn/{courseId}`):** The interface for taking a course, with lesson navigation, content display, and progress tracking.
-   **`Profile.razor` (`/user/profile`)**: The user's profile page, where they can update their information and view their achievements and certificates.

#### Admin Section (`/Components/Pages/Admin/`)

-   **`Dashboard.razor` (`/admin/dashboard`)**: An overview dashboard for administrators.
-   **`UserManagement.razor` (`/admin/users`)**: A page for managing all users.
-   **`CourseManagement.razor` (`/admin/courses`)**: A page for creating and managing courses, and linking to the Roster and Gradebook.
-   **`Gradebook.razor` (`/admin/courses/{id}/gradebook`)**: The matrix view for instructors to see all student grades in a course.
-   **`AchievementManagement.razor` (`/admin/achievements`)**: A page for managing achievements and their criteria.
-   **`GradeSubmissions.razor` (`/admin/grading`)**: A page for instructors to manually grade assessments.
-   **`CategoryManagement.razor` (`/admin/categories`)**: A page for managing course categories.
-   **`TagManagement.razor` (`/admin/tags`)**: A page for managing tags.
-   **`Settings.razor` (`/admin/settings`)**: A page for site-wide settings.

#### Report Section (`/Components/Pages/Report/`)

-   **`StudentEngagement.razor` (`/reports/student-engagement`)**: Displays charts related to student activity.
-   **`CourseCompletion.razor` (`/reports/course-completion`)**: Shows data on course completion rates.

#### Utility Pages

-   **`Error.razor`**: The default page for handling unhandled exceptions.
-   **`DiagnosticInfo.razor`**: A page for displaying diagnostic information.
-   **`TestError.razor`**: A page used to test the exception handling process.

## 3. Key Reusable Components

### `/Components/Shared/`

-   **`NotificationBell.razor` (to be created)**: A component in the top navigation that displays a badge for unread notifications.
-   **`FilePickerModal.razor` (to be created)**: A reusable modal for the central file management system.
-   **`DataGrid.razor` (to be created)**: A generic, reusable data grid component for admin tables.
-   **`Chart.razor` (to be created)**: A wrapper component for a charting library.
-   **`ConfirmationDialog.razor` (to be created)**: A standard dialog for confirming destructive actions.