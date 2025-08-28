# 17. Notifications and Events

This document outlines the architecture for a centralized notification system and a user-facing calendar.

## 1. Centralized Notification System

To provide timely and consistent communication, the system uses a centralized `NotificationService`.

### Data Model (`Notifications` Table)

| Field | Type | Description |
| --- | --- | --- |
| `Id` | `int` | Primary Key. |
| `RecipientUserId` | `string` | Foreign key to the `Users` table. |
| `Message` | `string` | The content of the notification. |
| `IsRead` | `bool` | Tracks if the user has viewed the notification. |
| `LinkUrl` | `string?` | An optional URL to navigate to when the notification is clicked. |
| `CreatedAt` | `DateTime` | Timestamp of when the notification was created. |

### The `NotificationService`

This is a central, backend service that other parts of the application call to create a notification. It is responsible for handling different delivery channels.

-   **Trigger Points**: The service is called from various places:
    -   `AnnouncementRepository`: When a new high-priority announcement is made.
    -   `GamificationService`: When a user earns an achievement.
    -   `CertificateService`: When a certificate is issued.
    -   `AssessmentEngine`: When an assignment has been manually graded.
-   **Delivery Channels**:
    1.  **In-App**: The service saves the notification to the `Notifications` table. It then uses a `NotificationHub` (SignalR) to push a real-time message to the client, which can update a "bell" icon with a badge count.
    2.  **Email**: If the user's preferences (`UserSettings`) allow it, the service can format and send an email.

## 2. Workflow: In-App Notification Delivery

1.  **Trigger**: A backend service (e.g., `GamificationService`) determines a notification is needed and calls `NotificationService.CreateNotification(userId, message, linkUrl)`.
2.  **Backend (`NotificationService`)**:
    -   A new `Notification` record is saved to the database with `IsRead = false`.
    -   The service then invokes the `NotificationHub` (SignalR).
3.  **Backend (`NotificationHub`)**:
    -   The hub sends a message to the specific user's client connection: `Clients.User(userId).SendAsync("NewNotificationReceived")`.
4.  **Frontend (`NotificationBell.razor`)**:
    -   The component listens for the `NewNotificationReceived` event.
    -   Upon receiving the event, it displays a badge on the bell icon, indicating unread notifications.
    -   When the user clicks the bell icon, the component calls `GET /api/notifications` to fetch and display the list of recent notifications.

## 3. Workflow: Instructor Adds a Custom Calendar Event

1.  **Navigation**: An instructor navigates to the calendar page for one of their courses.
2.  **UI**: They click an "Add Event" button.
3.  **Data Entry**: A form appears to enter the event `Title`, `Description`, `StartDate`, and `EndDate`.
4.  **API Call**: The instructor saves the event, which calls `POST /api/calendar-events` with the event details and the `RelatedCourseId`.
5.  **Result**: The new event is saved and will now appear on the calendar for the instructor and all students in that course.

## 4. Calendar / Scheduling System

The calendar provides users with a single place to view all important dates and deadlines.

### Data Model (`CalendarEvents` Table)

| Field | Type | Description |
| --- | --- | --- |
| `Id` | `int` | Primary Key. |
| `Title` | `string` | The title of the event. |
| `Description` | `string?` | An optional description. |
| `StartDate` | `DateTime` | The start time of the event. |
| `EndDate` | `DateTime` | The end time of the event. |
| `OwnerUserId` | `string?` | The user who created the event (if it's a custom event). Null for system-generated events. |
| `RelatedCourseId` | `int?` | Optional link to a course. |
| `EventType` | `enum` | The type of event (e.g., `AssessmentDeadline`, `LiveSession`, `Custom`). |

### Functionality

-   **Automatic Event Creation**: The system automatically populates the calendar with events based on other data:
    -   When an `Assessment` with a due date (`AvailableUntil`) is created, a corresponding `CalendarEvent` is added.
    -   When a `Course` with a start/end date is created, events are added.
-   **Manual Event Creation**: Instructors can add custom events for their courses (e.g., "Live Q&A Session at 7 PM").
-   **Personalized View**: The main calendar UI (`/calendar`) is powered by a `GET /api/calendar` endpoint that returns all events from all of a user's enrolled courses, plus any custom events they have created themselves.
