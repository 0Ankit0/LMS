# System Architecture

This document outlines the high-level architecture of the Learning Management System (LMS).

## 1. Solution Structure

The solution is built using .NET 8 and is structured following the principles of clean architecture and domain-driven design. It consists of several projects, each with a distinct responsibility.

-   **LMS.AppHost**: The main entry point for the application during development. It orchestrates the startup of the various services and the web front end, using .NET Aspire.
-   **LMS.Web**: The user-facing web application. It is a Blazor Server application responsible for rendering the UI and handling user interactions. It contains all the UI components, pages, and API endpoint definitions.
-   **LMS.Data**: The data access layer. It defines the database context, entities (domain models), and Data Transfer Objects (DTOs). It is responsible for all communication with the database.
-   **LMS.ServiceDefaults**: A project that contains default configurations for services, such as health checks, logging, and telemetry.
-   **LMS.Tests**: Contains unit and integration tests for the solution.

## 2. Technology Stack

-   **Backend**: .NET 8, ASP.NET Core, Entity Framework Core
-   **Frontend**: Blazor Server
-   **UI Components**: MudBlazor
-   **Styling**: Tailwind CSS
-   **Database**: SQL Server (or SQLite for development)
-   **Orchestration**: .NET Aspire
-   **Real-time Communication**: SignalR (for live UI updates)
-   **Asynchronous Processing**: A message queue (e.g., RabbitMQ, Azure Service Bus) for decoupling heavy background tasks like leaderboard calculations.

## 3. Architectural Principles

-   **Separation of Concerns**: Each project has a clear and distinct responsibility. The UI is separate from the data layer, and the business logic is encapsulated within services and repositories.
-   **Dependency Injection**: The solution makes extensive use of dependency injection to manage dependencies between components, promoting loose coupling and testability.
-   **Repository Pattern**: The `LMS.Web` project uses repositories to abstract the data access logic, making the application easier to maintain and test.
-   **Minimal APIs**: API endpoints are defined using ASP.NET Core's Minimal API framework, which provides a clean and efficient way to build HTTP APIs.
-   **Event-Driven Logic via EF Core Interceptors**: The system uses EF Core interceptors to create a data-driven architecture. Critical subsystems like Gamification are triggered by database changes (e.g., an update to a `Progress` record) rather than explicit calls from the business logic. This promotes decoupling and robustness.

## 4. High-Level Flow

1.  A user accesses the **LMS.Web** application through their browser.
2.  The Blazor Server application renders the requested page.
3.  If the page needs data, it calls a method on a repository (e.g., `CourseRepository`).
4.  The repository in **LMS.Web** uses the `ApplicationDbContext` from **LMS.Data** to query the database.
5.  The data is returned as entities, which are then mapped to DTOs or models for use in the UI.
6.  For client-side interactions that require backend logic (e.g., submitting a form), the Blazor component invokes a C# method, which in turn calls the appropriate API endpoint.
7.  The API endpoint handles the request, performs the necessary operations (e.g., saving data to the database), and returns a response.
