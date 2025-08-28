# 12. Assessment Engine

This document details the complete workflow for creating, taking, and grading assessments with various question types.

## 1. Data Model & Question Types

The assessment engine is built around the `Assessment`, `Question`, `QuestionOption`, `AssessmentAttempt`, and `QuestionResponse` tables. The key to supporting different test formats is the `Question.QuestionType` enum.

**Supported Question Types:**

-   **`MultipleChoice`**: A question with several options where only one is correct.
-   **`TrueFalse`**: A binary choice question.
-   **`ShortAnswer`**: Requires the user to type a brief text response.
-   **`Essay`**: Requires a longer, free-form text response.
-   **`Matching`**: Requires matching items from two lists.
-   **`FillInTheBlank`**: Requires filling in missing words in a sentence.
-   **`Ordering`**: Requires arranging items in a correct sequence.

## 2. Student Workflow: Taking an Assessment

1.  **UI Rendering**: When a student starts an assessment, the frontend will render a specific component for each question based on its `QuestionType`.
    -   `MultipleChoice` and `TrueFalse` questions will be rendered with radio buttons.
    -   `ShortAnswer` and `Essay` questions will be rendered with a text input box (`<textarea>`).
    -   Other types like `Matching` or `Ordering` will have specialized drag-and-drop or selection UIs.
2.  **Data Submission**: When the student submits the assessment, the frontend sends a list of answers to the `POST /api/assessments/{id}/submit` endpoint. 
    -   For multiple-choice answers, the `SelectedOptionId` field is populated in the `QuestionResponse`.
    -   For text-based answers, the `TextResponse` field is populated.

## 3. System Workflow: Grading an Assessment

Grading is a two-stage process that handles both automatically and manually graded questions.

### Stage 1: Automatic Grading

1.  **Trigger**: Immediately after a user submits an assessment.
2.  **Logic**: The backend grading service iterates through each `QuestionResponse` in the submission.
    -   For question types like `MultipleChoice`, `TrueFalse`, `Matching`, etc., the system has a defined correct answer.
    -   It compares the user's response to the stored correct answer and sets the `IsCorrect` and `PointsEarned` fields automatically.
    -   For question types like `ShortAnswer` and `Essay`, the system cannot determine correctness. It flags these responses for manual review (e.g., `IsCorrect = null`).
3.  **Result**: The `AssessmentAttempt` is saved with a status of `PendingManualGrading` if it contains any manually graded questions. If all questions were graded automatically, the status is set to `Completed` and the final score is calculated.

### Stage 2: Manual Grading

1.  **Instructor UI**: A new "Grade Submissions" area is available to instructors and admins. It shows a list of all assessment attempts with a status of `PendingManualGrading`.
2.  **Action**: The instructor selects a submission to grade.
3.  **Grading Interface**: The UI displays the student's submission. For each `ShortAnswer` or `Essay` question, it shows:
    -   The original question text.
    -   The student's `TextResponse`.
    -   A field for the instructor to enter the `PointsEarned`.
    -   Buttons to mark the response as Correct or Incorrect.
4.  **API Call**: The instructor submits the grades. The frontend calls a new endpoint, e.g., `POST /api/assessment-attempts/{id}/grade`, with the grading data for the manual questions.
5.  **Finalization**: The backend updates the remaining `QuestionResponse` records. Once all questions are graded, it recalculates the final `Score` for the `AssessmentAttempt`, sets its status to `Completed`, and triggers any downstream effects (like awarding achievements or updating course progress).
