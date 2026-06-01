# 11. Gamification Engine

This document provides a detailed explanation of the LMS's gamification features, including achievements, points, and leaderboards.

## 1. Overview & Data Model

The goal of the gamification engine is to increase user engagement by rewarding specific actions and fostering competition. The system is built on four key data models:

-   **`Achievement`**: The reward itself (e.g., "Course Pro"). It has a name, description, icon, and the number of points it's worth.
-   **`AchievementCriteria`**: The "rules engine" for achievements. Each record defines a specific, measurable condition that must be met to earn an achievement (e.g., `CriteriaType = CourseCompletion`, `CourseId = 123`).
-   **`UserAchievement`**: A simple record indicating that a specific user has earned a specific achievement on a specific date.
-   **`Leaderboard` & `LeaderboardEntry`**: Stores a periodic snapshot of the users with the most points.

## 2. Workflow: Instructor Creates an Achievement

This workflow details how an instructor or admin can define a new achievement and the rules to obtain it.

1.  **Navigation**: An instructor navigates to a course's settings page and selects the "Achievements" tab.
2.  **Action**: Clicks "Create New Achievement".
3.  **Data Entry (Achievement)**: The instructor fills in the achievement's details:
    -   **Name**: "Test Master"
    -   **Description**: "Awarded for passing the final exam with a perfect score."
    -   **Points**: `250`
4.  **Action**: The instructor saves the achievement. They are then prompted to add the rules that will trigger it.
5.  **Data Entry (Criteria)**: The instructor clicks "Add Rule". A form appears:
    -   **Rule Type**: They select `AssessmentScore` from a dropdown.
    -   **Assessment**: A dropdown appears showing all assessments in the course. They select "Final Exam".
    -   **Min Score (%)**: They enter `100`.
6.  **API Call**: The instructor saves the rule. The frontend calls `POST /api/achievements/{id}/criteria` with the rule details. The system now knows about this achievement and its trigger condition.

## 3. System Workflow: Automatic Achievement Awarding

This is the core backend process. It is not triggered by direct calls from the business logic, but rather by an event-driven mechanism that watches for changes in user progress.

### The EF Core Interceptor Trigger

The system uses an `Entity Framework Core Interceptor` to create a data-driven architecture. This interceptor automatically triggers the gamification logic whenever user progress is saved, ensuring no achievement checks are ever missed.

1.  **Intercepting `SaveChanges`**: The custom interceptor hooks into the `DbContext.SaveChangesAsync()` method.
2.  **Detecting Changes**: Before the changes are committed to the database, the interceptor inspects the entities being saved. It identifies all `Progress` entities that have been added or modified.
3.  **Executing the Logic**: After the changes are successfully saved to the database, the interceptor loops through the list of modified `Progress` entities it collected.
4.  **Invoking the Service**: For each changed `Progress` entity, the interceptor invokes the `GamificationService`, passing the context of the change (e.g., the user ID, the course ID, the new progress percentage, etc.).

### The `GamificationService`

This service is responsible for processing the event and awarding achievements. Its primary method is `CheckAndAwardAchievements(progressContext)`.

#### Internal Logic

When `CheckAndAwardAchievements` is called by the interceptor:

1.  The service uses the `progressContext` to understand what changed (e.g., a course was completed, a lesson was passed, etc.).
2.  It fetches all `AchievementCriteria` that could possibly be met by this type of progress change.
3.  It loops through these rules, first checking if the user has already been awarded the parent achievement.
4.  If not, it evaluates the rule's specific conditions against the user's new progress state.
5.  If all conditions for a rule are met, the service:
    a.  Creates a new record in the `UserAchievements` table.
    b.  Adds the achievement's point value to the `User.TotalPoints` field.
    c.  (Optional) Sends a real-time notification to the user about the new achievement.

### Benefits of this Approach

-   **Decoupling**: The core application logic (like completing a lesson) is completely unaware of the gamification system. It simply saves progress.
-   **Centralization**: All achievement logic is triggered from a single, consistent place.
-   **Robustness**: Achievements can never be missed. Any code path that results in saving progress will automatically and implicitly trigger the check.

## 4. System Workflow: Real-Time Leaderboard Updates

To provide an engaging, live experience, the leaderboard is updated in near real-time without impacting the performance of user-facing actions. This is achieved by decoupling the leaderboard calculation from the initial action.

### The Real-Time Gamification Pipeline

1.  **Action & Instant Update (User Request Thread)**:
    -   A user completes an action (e.g., passes a test).
    -   The `GamificationService` is called, which instantly awards the `UserAchievement` and updates the `User.TotalPoints` in the database. This is a fast, synchronous operation.

2.  **Publish Event (Decoupling)**:
    -   After successfully saving the new point total, the `GamificationService` publishes a message to a message queue (e.g., RabbitMQ, Azure Service Bus). 
    -   The message is lightweight, containing only essential information: `{ "UserId": "...", "NewTotalPoints": ... }`.
    -   This concludes the initial user request. The user gets a fast response from the API.

3.  **Process Leaderboard (Background Worker Thread)**:
    -   A separate, continuously running background service (a "Worker Service") is subscribed to the message queue.
    -   When it receives a message, it performs an optimized leaderboard update:
        a.  It checks the score of the lowest-ranked user currently on the leaderboard (e.g., the user at rank #100).
        b.  If the new score from the message is not higher than the lowest score, no leaderboard change is needed, and the process stops.
        c.  If the new score is higher, the service updates the `LeaderboardEntry` table by inserting the user at their new correct rank and removing the user who is now ranked #101.

4.  **Push Live Update to Clients (Real-Time Communication)**:
    -   After the database is updated, the Worker Service uses a SignalR Hub.
    -   It broadcasts a simple notification, like `"LeaderboardUpdated"`, to a specific group of clients—only those who are currently viewing the leaderboard page.
    -   The Leaderboard.razor component on the frontend listens for this event. When it receives the notification, it automatically re-fetches the leaderboard data from `GET /api/leaderboard` and the UI updates live, without a page refresh.

