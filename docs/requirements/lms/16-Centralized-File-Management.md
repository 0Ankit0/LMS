# 16. Centralized File Management

This document outlines the architecture for a centralized media library, allowing users to manage all their uploaded files in one place.

## 1. Overview & Rationale

Instead of having separate upload buttons for every feature (profile pictures, course thumbnails, lesson resources), all file uploads will be managed through a single, central library for each user. This improves usability, reduces duplicate uploads, and allows for robust data integrity checks.

## 2. Data Model

A new `UserFile` table is introduced to store metadata for every file uploaded by any user.

**`UserFile` Table**

| Field | Type | Description |
| --- | --- | --- |
| `Id` | `int` | Primary Key. |
| `OwnerUserId` | `string` | Foreign key to the `Users` table. |
| `FileName` | `string` | The original name of the file. |
| `FilePath` | `string` | The path to the file on the server or in cloud storage. |
| `FileSize` | `long` | The size of the file in bytes. |
| `ContentType` | `string` | The MIME type of the file (e.g., "image/png"). |
| `UploadedAt` | `DateTime` | Timestamp of the upload. |

**Schema Refactoring**

All other tables that previously stored a file URL will be modified to store a nullable foreign key to the `UserFile` table. For example:
-   `Users.ProfilePictureUrl` becomes `Users.ProfilePictureFileId` (int?)
-   `Courses.ThumbnailUrl` becomes `Courses.ThumbnailFileId` (int?)
-   `LessonResource.FilePath` becomes `LessonResource.FileId` (int?)

## 3. The File Manager UI

-   **Trigger**: A user clicks any button to add a file or image (e.g., "Change Profile Picture", "Add Lesson Document").
-   **Component**: A reusable `FilePickerModal.razor` component is displayed.
-   **Modal View**: The modal displays a gallery of all files from the user's `UserFile` library. Users can:
    -   **Select an existing file**: Clicking a file selects it and closes the modal, returning the `UserFile.Id` to the parent component.
    -   **Upload a new file**: An "Upload New" button allows for new uploads, which adds the file to the library and automatically selects it.
    -   **Manage files**: Users can select one or more of their files and click a "Delete" button.

## 4. The Deletion Workflow

This is the most critical part of the system, ensuring that in-use files are not deleted without the user's explicit consent. This workflow is designed to handle batch deletions efficiently.

1.  **Trigger**: Inside the File Manager, the user selects one or more files and clicks "Delete".
2.  **API Call**: The frontend sends a single request to a batch delete endpoint: `POST /api/my-files/delete-batch`.
    -   **Request Body**: `{ "fileIds": [1, 2, 3] }`
3.  **Backend - Usage Check**:
    -   The API endpoint receives the list of file IDs.
    -   For each ID in the list, it performs the "usage check" using the `FileUsageService`.
    -   **If no usages are found for any files**: All physical files are deleted, the `UserFile` records are deleted, and the API returns a `200 OK`.
    -   **If any file is found to be in use**: The API **does not delete any files**. Instead, it returns a `409 Conflict` error. The response body contains a list of conflict reports, one for each file that is in use: `[{ "fileId": 2, "fileName": "b.jpg", "usages": [...] }]`.
4.  **Frontend - Deletion Prompt**:
    -   The UI receives the `409 Conflict` response. It clearly indicates to the user which files could not be deleted and why, showing the list of conflicts.
    -   It then presents the user with the same choices as before for resolving these conflicts.
5.  **Frontend - Second API Call**:
    -   If the user chooses to proceed, the frontend calls the same batch delete endpoint again, but includes the full list of original IDs and the conflict resolution strategy: `POST /api/my-files/delete-batch`
    -   **Request Body**: `{ "fileIds": [1, 2, 3], "onConflict": "setNull" }`
6.  **Backend - Final Deletion**:
    -   The API receives the second request.
    -   It processes the full list of files again. For files with no conflicts, it deletes them directly. For files that do have conflicts, it applies the specified `onConflict` policy (e.g., `setNull` or `deleteData`).
    -   Finally, it deletes the physical files and the `UserFile` records.
