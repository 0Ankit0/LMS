# 18. Project Structure and Conventions

This document outlines recommended conventions based on the current project structure to ensure consistency and maintainability as the project grows.

## 1. Solution and Project Structure

The solution follows the principles of Clean Architecture, with dependencies flowing inwards towards the domain entities.

-   `LMS.Data`: The core data layer. **It should contain all database entities and all Data Transfer Objects (DTOs).**
-   `LMS.Web`: The presentation layer (Blazor UI). It should contain all UI components, pages, and repositories. It depends on `LMS.Data` but not the other way around.
-   `LMS.AppHost` & `LMS.ServiceDefaults`: .NET Aspire orchestration projects.

## 2. Data Transfer Object (DTO) Consolidation

**Observation**: The project currently has DTOs defined in both `LMS.Data/DTOs` and `LMS.Web/Repositories/DTOs`.

**Recommendation**: To maintain a clean separation of concerns, **all DTOs should be consolidated into the `LMS.Data` project.**

**Rationale**:
-   **Single Source of Truth**: Having all data contract definitions in one place prevents duplication and confusion.
-   **Clear Dependency Flow**: The Web/UI layer should depend on the Data layer for its models. The Data layer should not need to know anything about the UI layer. Placing DTOs in the Web project would violate this principle if a service in another project ever needed access to them.

## 3. Configuration

-   Application settings, connection strings, and other environmental configurations should be managed in `appsettings.json` and its environment-specific overrides (e.g., `appsettings.Development.json`).

## 4. Static Assets

-   All public-facing static assets, such as images, CSS files, and fonts, must be placed in the `LMS.Web/wwwroot` directory.
-   User-uploaded content that needs to be publicly accessible (like course thumbnails or profile pictures) should be stored in a designated subfolder within `wwwroot`, such as `wwwroot/uploads/images`.
