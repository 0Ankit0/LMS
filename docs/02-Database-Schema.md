# 2. Database Schema

This document details the database schema for the LMS, based on the entities defined in the `LMS.Data` project.

## Shared Tables

### `UserFiles`

A central library for all files uploaded by users.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `OwnerUserId` | `string` | Foreign key to the `Users` table. | Required; On Delete: Cascade (All files are deleted with the user). |
| `FileName` | `string` | The original name of the file. | Required; Max Length: 255 |
| `FilePath` | `string` | The path to the file on the server or in cloud storage. | Required; Max Length: 1024 |
| `FileSize` | `long` | The size of the file in bytes. | Required; Range: > 0 |
| `ContentType` | `string` | The MIME type of the file. | Required; Max Length: 100 |
| `UploadedAt` | `DateTime` | Timestamp of the upload. | Required; Default: `Now()` |

## User Management

### `Users` (IdentityUser)

Stores user account information, extending the default ASP.NET Core IdentityUser.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `string` | Primary Key. | - |
| `FirstName` | `string` | User's first name. | Required; Max Length: 100 |
| `LastName` | `string` | User's last name. | Required; Max Length: 100 |
| `Bio` | `string` | A short biography of the user. | Max Length: 500 |
| `ProfilePictureFileId` | `int?` | Foreign key to the `UserFiles` table for the user's profile picture. | On Delete: Set Null |
| `DateOfBirth` | `DateTime` | User's date of birth. | Required |
| `IsActive` | `bool` | Flags if the user account is active. | Required; Default: `true` |
| `CreatedAt` | `DateTime` | Timestamp of account creation. | Required; Default: `Now()` |
| `LastLoginAt` | `DateTime?` | Timestamp of the user's last login. | - |
| `TotalPoints` | `int` | Total points earned through gamification. | Required; Default: 0; Range: >= 0 |
| `Level` | `int` | User's current level in the gamification system. | Required; Default: 1; Range: >= 1 |
| `StudentIdNumber` | `string?` | An official student ID number, for SMS integration. | Max Length: 50; Unique (if not null) |
| `EnrollmentDate` | `DateTime?` | The date the student first enrolled in the institution. | - |
| `GraduationDate` | `DateTime?` | The student's graduation date. | - |
| `EmergencyContactName` | `string?` | Name of an emergency contact. | Max Length: 200 |
| `EmergencyContactPhone` | `string?` | Phone number for an emergency contact. | Max Length: 50 |
| ... | ... | Other standard IdentityUser fields (UserName, Email, etc.) | `Email` is Required, Unique. `UserName` is Required, Unique. |

### `UserSettings`

Stores user-specific preferences.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `UserId` | `string` | Foreign key to the `Users` table. | Required; Primary Key; On Delete: Cascade |
| `Theme` | `string` | The preferred UI theme (e.g., "Light", "Dark"). | Required; Max Length: 50; Default: "Light" |
| `Language` | `string` | The preferred language (e.g., "en-US"). | Required; Max Length: 10; Default: "en-US" |
| `EmailNotifications` | `bool` | Preference for receiving email notifications. | Required; Default: `true` |
| `SmsNotifications` | `bool` | Preference for receiving SMS notifications. | Required; Default: `false` |
| `PushNotifications` | `bool` | Preference for receiving push notifications. | Required; Default: `true` |

### `UserActivity`

Logs significant user actions within the system.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `UserId` | `string` | Foreign key to the `Users` table. | Required; On Delete: Cascade |
| `Type` | `ActivityType` | The type of activity performed. | Required |
| `Description` | `string` | A description of the activity. | Required; Max Length: 500 |
| `Timestamp` | `DateTime` | When the activity occurred. | Required; Default: `Now()` |
| `IpAddress` | `string` | IP address from which the activity originated. | Max Length: 50 |
| `UserAgent` | `string` | The user agent string of the client. | Max Length: 500 |

## Course Structure

### `Courses`

The main table for courses.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Title` | `string` | The title of the course. | Required; Max Length: 200 |
| `Description` | `string` | A detailed description of the course. | Required; Max Length: 4000 |
| `ThumbnailFileId` | `int?` | Foreign key to the `UserFiles` table for the course's thumbnail image. | On Delete: Set Null |
| `InstructorId` | `string` | Foreign key to the `Users` table for the course instructor. | Required; On Delete: Restrict |
| `Level` | `CourseLevel` | The difficulty level of the course. | Required; Default: `Beginner` |
| `Status` | `CourseStatus` | The current status of the course. | Required; Default: `Draft` |
| `StartDate` | `DateTime` | The official start date of the course. | Required |
| `EndDate` | `DateTime?` | The official end date of the course. | - |
| ... | ... | | |

### `Modules`

Courses are divided into modules.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Title` | `string` | The title of the module. | Required; Max Length: 200 |
| `Description` | `string` | A description of the module. | Max Length: 4000 |
| `CourseId` | `int` | Foreign key to the `Courses` table. | Required; On Delete: Cascade |
| `OrderIndex` | `int` | The display order of the module within the course. | Required; Default: 0 |
| `GatingAssessmentId` | `int?` | Optional foreign key to an `Assessment` that must be passed to complete this module. | On Delete: Set Null |
| `MustPassGatingAssessment` | `bool` | If true, the user must pass the `GatingAssessmentId` to mark the module as complete. | Required; Default: `false` |
| ... | ... | | |

### `Lessons`

Modules are composed of lessons (referred to as "topics" by the user). The `LessonType` enum and the various content fields allow for a wide variety of content.

- **Text with Images**: Stored as HTML in the `Content` field.
- **Internal Video**: The `VideoUrl` field would point to a path on the server (e.g., `/uploads/videos/lesson1.mp4`).
- **External Website Link**: The `ExternalUrl` field would store the full URL. The `LessonType` would be `External`.
- **File Download**: A `LessonResource` of type `Document` would be used.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Title` | `string` | The title of the lesson. | Required; Max Length: 200 |
| `Content` | `string` | The main content of the lesson (e.g., HTML for text and images). | - |
| `ModuleId` | `int` | Foreign key to the `Modules` table. | Required; On Delete: Cascade |
| `Type` | `LessonType` | The type of lesson (e.g., Text, Video, External). | Required; Default: `Text` |
| `VideoUrl` | `string?` | URL to a video if the lesson type is video. | Max Length: 1024 |
| `ExternalUrl` | `string?` | URL to an external website. | Max Length: 1024 |
| `OrderIndex` | `int` | The display order of the lesson within the module. | Required; Default: 0 |
| `GatingAssessmentId` | `int?` | Optional foreign key to an `Assessment` that must be passed to complete this lesson. | On Delete: Set Null |
| `MustPassGatingAssessment` | `bool` | If true, the user must pass the `GatingAssessmentId` to mark the lesson as complete. | Required; Default: `false` |
| ... | ... | | |

### `LessonResources`

Files or links associated with a lesson.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Name` | `string` | The display name of the resource. | Required; Max Length: 200 |
| `LessonId` | `int` | Foreign key to the `Lessons` table. | Required; On Delete: Cascade |
| `Type` | `ResourceType` | The type of resource (e.g., Document, Link). | Required; Default: `Document` |
| `FileId` | `int?` | Foreign key to the `UserFiles` table. Used when the resource is a file. | On Delete: Set Null |
| `ExternalUrl` | `string?` | Used when the resource is a link to an external site. | Max Length: 1024 |
| ... | ... | | |

### `Categories` & `Tags`

For organizing and classifying courses.

-   **Categories**: A hierarchical structure for grouping courses (e.g., "Programming" -> "Web Development"). Each category can have an icon represented by a `UserFile`.
-   **Tags**: Non-hierarchical keywords for courses (e.g., "C#", ".NET").
-   **CourseCategory** & **CourseTags**: Junction tables for the many-to-many relationships.

## Enrollment & Progress

### `Enrollments`

Tracks which users are enrolled in which courses.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `UserId` | `string` | Foreign key to the `Users` table. | Required; On Delete: Cascade |
| `CourseId` | `int` | Foreign key to the `Courses` table. | Required; On Delete: Cascade |
| `EnrolledAt` | `DateTime` | Timestamp of when the enrollment occurred. | Required; Default: `Now()` |
| `CompletedAt` | `DateTime?` | Timestamp of when the course was completed. | - |
| `Status` | `EnrollmentStatus` | The status of the enrollment. | Required; Default: `Active` |
| `ProgressPercentage` | `double` | The overall progress percentage for the course. | Required; Default: 0; Range: 0-100 |
| ... | ... | | **Unique constraint on (`UserId`, `CourseId`)** |

### `ModuleProgress` & `LessonProgress`

Track a user's progress through a course at the module and lesson level. These tables link back to the `Enrollments` table. Both tables follow a similar structure with validation rules for percentages (0-100) and required foreign keys.

## Assessments

### `Assessments`

Defines quizzes, exams, or assignments.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Title` | `string` | The title of the assessment. | Required; Max Length: 200 |
| `CourseId` | `int?` | The course this assessment belongs to. | On Delete: Cascade |
| `ModuleId` | `int?` | The module this assessment belongs to. | On Delete: Cascade |
| `LessonId` | `int?` | The lesson this assessment belongs to. | On Delete: Cascade |
| `Type` | `AssessmentType` | The type of assessment. | Required; Default: `Quiz` |
| `MaxAttempts` | `int` | Maximum number of attempts allowed. | Required; Default: 1; Range: >= 0 (0 for unlimited) |
| `TimeLimit` | `TimeSpan` | The time limit for the assessment. | Required; Default: 0 (0 for no limit) |
| `PassingScore` | `double` | The minimum score required to pass. | Required; Default: 70; Range: 0-100 |
| ... | ... | | |

### `Questions` & `QuestionOptions`

-   **Questions**: Stores the questions for an assessment.
-   **QuestionOptions**: Stores the possible answers for a multiple-choice question.

### `AssessmentAttempts` & `QuestionResponses`

-   **AssessmentAttempts**: Records each attempt a user makes on an assessment. `EnrollmentId` and `AssessmentId` are required foreign keys.
-   **QuestionResponses**: Records the specific answers a user provided for each question in an attempt. `AttemptId` and `QuestionId` are required foreign keys.

## Communication

### `Announcements`

For broadcasting information to users.

| Field | Type | Description |
| --- | --- | --- |
| `Id` | `int` | Primary Key. |
| `Title` | `string` | The title of the announcement. |
| `Content` | `string` | The body of the announcement. |
| `AuthorId` | `string` | Foreign key to the `Users` table. |
| `CourseId` | `int?` | Optional: scopes the announcement to a specific course. |
| `Type` | `AnnouncementType` | The type of announcement (e.g., General, Event). |
| `PublishedAt` | `DateTime` | When the announcement is made public. |
| ... | ... | |

### `Conversations`

Represents a private chat session between two or more users.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Title` | `string?` | Optional title for group chats. | Max Length: 200 |
| `CreatedAt` | `DateTime` | Timestamp of when the conversation was created. | Required; Default: `Now()` |
| `CreatorId` | `string` | The user who initiated the conversation. | Required; On Delete: Set Null |

### `ConversationParticipants`

Junction table linking users to conversations, defining who is in a chat.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `ConversationId` | `int` | Foreign key to the `Conversations` table. | Required; On Delete: Cascade |
| `UserId` | `string` | Foreign key to the `Users` table. | Required; On Delete: Cascade |
| | | | Unique constraint on (`ConversationId`, `UserId`) |

### `MessageAttachments`

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `MessageId` | `int` | Foreign key to the `Messages` table. | Required; On Delete: Cascade |
| `FileId` | `int` | Foreign key to the `UserFiles` table. | Required; On Delete: Restrict |

### `Messages`

For private or group communication between users. Content is End-to-End Encrypted.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `ConversationId` | `int` | Foreign key to the `Conversations` table. | Required; On Delete: Cascade |
| `SenderId` | `string` | Foreign key to the `Users` table for the sender. | Required; On Delete: Set Null |
| `Content` | `string` | The **encrypted** message content. The server cannot read this. | Required |
| `SentAt` | `DateTime` | Timestamp of when the message was sent. | Required; Default: `Now()` |
| ... | ... | | |

### `Forums`, `ForumTopics`, `ForumPosts`, `ForumAttachments`

Provides discussion forums for courses or general topics.

-   **Forums**: The top-level container for discussions. Can be general or course-specific.
-   **ForumTopics**: A single discussion thread within a forum.
-   **ForumPosts**: An individual post or reply within a topic.
-   **ForumAttachments**: A junction table linking a `ForumPost` to a `UserFile`.

## Gamification

### `Achievements`

Defines badges or awards users can earn.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `Name` | `string` | The name of the achievement. | Required; Max Length: 100; Unique |
| `Description` | `string` | How to earn the achievement. | Required; Max Length: 500 |
| `IconFileId` | `int?` | Foreign key to the `UserFiles` table for the achievement's badge icon. | On Delete: Set Null |
| `Points` | `int` | Points awarded for this achievement. | Required; Default: 0; Range: >= 0 |
| ... | ... | | |

### `UserAchievements`

Junction table tracking which users have earned which achievements.

### `Leaderboards` & `LeaderboardEntries`

-   **Leaderboards**: Defines different leaderboards (e.g., "Top Points - Weekly").
-   **LeaderboardEntries**: Stores the rank and score for each user on a leaderboard.

### `Certificates`

Stores information about certificates issued to users upon course completion.

| Field | Type | Description | Validation Rules & Constraints |
| --- | --- | --- | --- |
| `Id` | `int` | Primary Key. | - |
| `UserId` | `string` | Foreign key to the `Users` table. | Required; On Delete: Cascade |
| `CourseId` | `int` | Foreign key to the `Courses` table. | Required; On Delete: Cascade |
| `CertificateNumber` | `string` | A unique number for the certificate. | Required; Max Length: 100; Unique |
| `IssuedAt` | `DateTime` | When the certificate was issued. | Required; Default: `Now()` |
| `CertificateFileId` | `int?` | A foreign key to the `UserFiles` table for the generated certificate PDF. | On Delete: Set Null |
| ... | ... | | Unique constraint on (`UserId`, `CourseId`) |
