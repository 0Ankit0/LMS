# 8. Detailed Admin Workflows

This document provides detailed, step-by-step guides for complex administrative tasks within the LMS.

## 1. Workflow: End-to-End Course Creation

**Goal**: An administrator creates a complete, structured course with modules, lessons, and a final quiz, and publishes it for users to enroll in.

**Actor**: An Administrator.

### Part 1: Create the Course Shell

1.  **Navigation**: Admin navigates to the Admin Dashboard -> Course Management (`/admin/courses`).
2.  **Action**: Clicks the "Create New Course" button.
3.  **UI**: A form appears (likely in a dialog or on a new page) with fields for the main course details.
4.  **Data Entry**: Admin fills in the required information:
    -   **Title**: e.g., "Introduction to Blazor"
    -   **Description**: A summary of the course.
    -   **Instructor**: Selects a user from a dropdown (populated by `GET /api/dropdown/users`).
    -   **Category**: Selects a category from a dropdown (populated by `GET /api/dropdown/categories`).
    -   **Tags**: Uses a multi-select autocomplete component. As the admin types, it suggests existing tags. The admin can select multiple tags (e.g., "C#", "Blazor", ".NET 8"). If a tag doesn't exist, they can create it on the fly.
    -   **Level**: Selects "Beginner".
    -   **Thumbnail**: Uploads a course image.
5.  **API Call**: Admin clicks "Save and Continue". The frontend calls `POST /api/courses` with the form data. The API returns the `id` of the newly created course.
6.  **Redirect**: The admin is redirected to the course content management page, e.g., `/admin/courses/edit/123`.

### Part 2: Build the Course Structure

1.  **UI**: The course editor page shows the course details at the top and an area for managing content, which is currently empty. It says, "This course has no modules. Add one to get started."
2.  **Action**: Admin clicks "Add Module".
3.  **Data Entry**: A small form appears to enter the module title (e.g., "1. Fundamentals") and description.
4.  **API Call**: Admin clicks "Save Module". The frontend calls `POST /api/courses/123/modules`.
5.  **UI Update**: The page refreshes, now showing the "1. Fundamentals" module with options to "Add Lesson" or "Edit Module".
6.  **Repeat**: The admin repeats steps 2-5 to create a second module, "2. Advanced Concepts".

### Part 3: Add Lessons to a Module

1.  **Action**: Under the "1. Fundamentals" module, the admin clicks "Add Lesson".
2.  **UI**: A form/dialog appears for creating a new lesson.
3.  **Data Entry (Text Lesson)**:
    -   **Title**: "What is Blazor?"
    -   **Lesson Type**: Selects "Text" from a dropdown.
    -   **Content**: The admin uses a rich text editor to input the lesson content.
4.  **API Call**: Admin clicks "Save Lesson". The frontend calls `POST /api/modules/456/lessons`.
5.  **UI Update**: The lesson "What is Blazor?" now appears under the "1. Fundamentals" module.
6.  **Action**: Admin clicks "Add Lesson" again.
7.  **Data Entry (Video Lesson)**:
    -   **Title**: "Component Lifecycle"
    -   **Lesson Type**: Selects "Video".
    -   **Video URL**: Pastes a URL from a video hosting provider (e.g., YouTube, Vimeo).
8.  **API Call**: Admin saves the lesson. The frontend calls the same `POST` endpoint, but the request body now contains the `VideoUrl` and a different `LessonType`.

### Part 4: Create an Assessment (Quiz)

1.  **Action**: Under the "2. Advanced Concepts" module, the admin clicks "Add Assessment".
2.  **UI**: A form appears to create the assessment shell.
3.  **Data Entry**: Admin fills in the details:
    -   **Title**: "Final Exam"
    -   **Type**: Selects "Quiz".
    -   **Passing Score**: Sets to `75` (percent).
    -   **Max Attempts**: Sets to `3`.
4.  **API Call**: Admin clicks "Save and Add Questions". The frontend calls `POST /api/assessments` (or a similar endpoint scoped to the course/module). The API returns the new `assessmentId`.
5.  **Redirect**: The admin is taken to the question editor for the new assessment, e.g., `/admin/assessments/edit/789`.

### Part 5: Add Questions to the Quiz

1.  **UI**: The assessment editor page shows the assessment details and an empty list of questions.
2.  **Action**: Admin clicks "Add Question".
3.  **Data Entry (Multiple Choice)**:
    -   **Question Type**: Selects "Multiple Choice".
    -   **Text**: "Which Blazor hosting model runs entirely on the client's browser?"
    -   **Points**: Sets to `10`.
    -   **Options**: The admin is presented with fields to add several options. They add:
        -   "Blazor Server" (Is Correct: false)
        -   "Blazor WebAssembly" (Is Correct: true)
        -   "Blazor Hybrid" (Is Correct: false)
4.  **API Call**: Admin clicks "Save Question". The frontend calls `POST /api/assessments/789/questions`.
5.  **UI Update**: The question now appears in the list.
6.  **Action**: Admin clicks "Add Question" again to add a different type.
7.  **Data Entry (Short Answer)**:
    -   **Question Type**: Selects "Short Answer".
    -   **Text**: "In one sentence, what is the primary role of a .razor file?"
    -   **Points**: Sets to `20`.
    -   The UI notes that this question type requires manual grading.
8.  **API Call**: Admin saves the question. The same endpoint is called, but the request body contains a different `QuestionType` and no options.
9.  **Repeat**: The admin adds any other questions needed for the quiz.

### Part 6: Gate a Module with the Quiz

1.  **Navigation**: Admin navigates back to the course editor (`/admin/courses/edit/123`).
2.  **Action**: On the "2. Advanced Concepts" module, the admin clicks an "Edit" or "Settings" icon.
3.  **UI**: A dialog or settings panel for the module appears.
4.  **Data Entry**: The admin checks a box labeled "Require an assessment to complete this module".
    -   A dropdown appears, populated with all assessments created for this course.
    -   The admin selects "Final Exam" from the list.
5.  **API Call**: Admin clicks "Save". The frontend calls `PUT /api/modules/457` (the ID of the module) with a request body like: `{ "gatingAssessmentId": 789, "mustPassGatingAssessment": true }`.
6.  **UI Update**: The module in the admin view now displays a small badge or icon indicating it is gated by an assessment (e.g., a lock icon).

### Part 7: Publish the Course

1.  **Navigation**: Admin goes back to the main course editor page (`/admin/courses/edit/123`).
2.  **Review**: The admin reviews the complete structure of modules, lessons, and the final quiz.
3.  **Action**: The course `Status` is currently "Draft". The admin clicks a "Publish" button.
4.  **API Call**: The frontend calls `PUT /api/courses/123` with a request body indicating the status change: `{ "status": "Published" }`.
5.  **Confirmation**: The course is now live and will appear in the public course catalog for users to discover and enroll in.
