# Project Requirements: LMS

This document outlines the core architectural and user experience requirements for the Learning Management System (LMS) project. The primary goal is to create a fast, interactive, and visually appealing application using Blazor and MudBlazor.

## Project Tree Structure

```
├── .gitattributes
├── .gitignore
├── LMS.sln
├── LMS
    ├── LMS.AppHost
    │   ├── LMS.AppHost.csproj
    │   ├── Program.cs
    │   ├── Properties
    │   │   └── launchSettings.json
    │   ├── appsettings.Development.json
    │   └── appsettings.json
    ├── LMS.Data
    │   ├── DTOs
    │   │   ├── Account
    │   │   │   ├── AssignUserModel.cs
    │   │   │   ├── ChangePasswordModel.cs
    │   │   │   ├── ClaimModel.cs
    │   │   │   ├── EmailInputModel.cs
    │   │   │   ├── EnableAuthenticatorInputModel.cs
    │   │   │   ├── LoginRequest.cs
    │   │   │   ├── MachineRememberedResponse.cs
    │   │   │   ├── PasswordInputModel.cs
    │   │   │   ├── PhoneInputModel.cs
    │   │   │   ├── ProfileInputModel.cs
    │   │   │   ├── RecoveryCodeLoginRequest.cs
    │   │   │   ├── RegisterModel.cs
    │   │   │   ├── RemoveLockoutRequest.cs
    │   │   │   ├── RequestLockoutCodeRequest.cs
    │   │   │   ├── ResetModel.cs
    │   │   │   ├── RoleModel.cs
    │   │   │   ├── SetPasswordModel.cs
    │   │   │   └── TwoFactorLoginRequest.cs
    │   │   ├── Common
    │   │   │   ├── DropdownOption.cs
    │   │   │   ├── PaginatedResult.cs
    │   │   │   └── TableColumn.cs
    │   │   ├── LMS
    │   │   │   ├── Assessment
    │   │   │   │   ├── AssessmentAttemptModel.cs
    │   │   │   │   ├── AssessmentModel.cs
    │   │   │   │   ├── CreateAssessmentModel.cs
    │   │   │   │   ├── CreateAssessmentRequest.cs
    │   │   │   │   ├── CreateQuestionModel.cs
    │   │   │   │   ├── CreateQuestionOptionModel.cs
    │   │   │   │   ├── CreateQuestionOptionRequest.cs
    │   │   │   │   ├── CreateQuestionRequest.cs
    │   │   │   │   ├── QuestionModel.cs
    │   │   │   │   ├── QuestionOptionModel.cs
    │   │   │   │   ├── QuestionResponseModel.cs
    │   │   │   │   ├── SubmitAssessmentModel.cs
    │   │   │   │   ├── SubmitAssessmentRequest.cs
    │   │   │   │   ├── SubmitQuestionResponseModel.cs
    │   │   │   │   └── SubmitQuestionResponseRequest.cs
    │   │   │   ├── Communication
    │   │   │   │   ├── AnnouncementModel.cs
    │   │   │   │   ├── CreateAnnouncementRequest.cs
    │   │   │   │   ├── CreateForumPostRequest.cs
    │   │   │   │   ├── CreateForumRequest.cs
    │   │   │   │   ├── CreateForumTopicRequest.cs
    │   │   │   │   ├── CreateMessageRequest.cs
    │   │   │   │   ├── ForumModel.cs
    │   │   │   │   ├── ForumPostModel.cs
    │   │   │   │   ├── ForumTopicModel.cs
    │   │   │   │   ├── MessageAttachmentModel.cs
    │   │   │   │   └── MessageModel.cs
    │   │   │   ├── Course
    │   │   │   │   ├── CategoryModel.cs
    │   │   │   │   ├── CourseModel.cs
    │   │   │   │   ├── CreateCourseRequest.cs
    │   │   │   │   ├── CreateLessonRequest.cs
    │   │   │   │   ├── CreateModuleRequest.cs
    │   │   │   │   ├── LessonModel.cs
    │   │   │   │   ├── LessonResourceModel.cs
    │   │   │   │   ├── ModuleModel.cs
    │   │   │   │   └── TagModel.cs
    │   │   │   └── User
    │   │   │   │   ├── AchievementModel.cs
    │   │   │   │   ├── CertificateModel.cs
    │   │   │   │   ├── CreateEnrollmentRequest.cs
    │   │   │   │   ├── EnrollmentModel.cs
    │   │   │   │   ├── LeaderboardEntryModel.cs
    │   │   │   │   ├── LeaderboardModel.cs
    │   │   │   │   ├── LessonProgressModel.cs
    │   │   │   │   ├── ModuleProgressModel.cs
    │   │   │   │   ├── UpdateProgressRequest.cs
    │   │   │   │   ├── UpdateUserProfileRequest.cs
    │   │   │   │   ├── UserAchievementModel.cs
    │   │   │   │   └── UserModel.cs
    │   │   └── Report
    │   │   │   ├── AssessmentPerformanceReportDto.cs
    │   │   │   ├── AttendanceReportDto.cs
    │   │   │   ├── CourseCompletionReportDto.cs
    │   │   │   ├── EnrollmentSummaryReportDto.cs
    │   │   │   ├── ForumActivityReportDto.cs
    │   │   │   ├── GradeDistributionReportDto.cs
    │   │   │   ├── StudentInformationReportDto.cs
    │   │   │   ├── StudentProgressReportDto.cs
    │   │   │   └── TeacherPerformanceReportDto.cs
    │   ├── DataSeeder.cs
    │   ├── Entities
    │   │   ├── Assessment.cs
    │   │   ├── Attendance.cs
    │   │   ├── Category.cs
    │   │   ├── Communication.cs
    │   │   ├── Course.cs
    │   │   ├── Enrollment.cs
    │   │   ├── Forum.cs
    │   │   ├── Gamification.cs
    │   │   ├── Lesson.cs
    │   │   ├── Module.cs
    │   │   ├── Progress.cs
    │   │   ├── Question.cs
    │   │   └── User.cs
    │   └── LMS.Data.csproj
    ├── LMS.ServiceDefaults
    │   ├── Extensions.cs
    │   └── LMS.ServiceDefaults.csproj
    ├── LMS.Tests
    │   ├── LMS.Tests.csproj
    │   └── WebTests.cs
    ├── LMS.Web
    │   ├── LMS.Web.Client
    │   │   ├── LMS.Web.Client.csproj
    │   │   ├── Layout
    │   │   │   ├── ClientLayout.razor
    │   │   │   ├── MainLayout.razor
    │   │   │   ├── MainLayout.razor.css
    │   │   │   ├── MainSidebar.razor
    │   │   │   ├── Navbar.razor
    │   │   │   └── mainSidebar.razor.css
    │   │   ├── Models
    │   │   │   ├── AchievementModels.cs
    │   │   │   └── SettingsModels.cs
    │   │   ├── Pages
    │   │   │   ├── Achievements.razor
    │   │   │   ├── Analytics.razor
    │   │   │   ├── Analytics.razor.css
    │   │   │   ├── AnnouncementList.razor
    │   │   │   ├── AnnouncementList.razor.css
    │   │   │   ├── Announcements.razor
    │   │   │   ├── Announcements.razor.css
    │   │   │   ├── Assessments.razor
    │   │   │   ├── Certificates.razor
    │   │   │   ├── CourseCard.razor
    │   │   │   ├── CourseCatalog.razor
    │   │   │   ├── CourseCatalog.razor.css
    │   │   │   ├── CourseDetails.razor
    │   │   │   ├── CourseDetails.razor.css
    │   │   │   ├── CourseListSearch.razor
    │   │   │   ├── Dashboard.razor
    │   │   │   ├── Forums.razor
    │   │   │   ├── Forums.razor.css
    │   │   │   ├── Home.razor
    │   │   │   ├── Leaderboard.razor
    │   │   │   ├── Messages.razor
    │   │   │   ├── MyCourses.razor
    │   │   │   └── Settings.razor
    │   │   ├── Program.cs
    │   │   ├── RedirectToLogin.razor
    │   │   ├── Routes.razor
    │   │   ├── Services
    │   │   │   ├── ClientToastService.cs
    │   │   │   └── IToastService.cs
    │   │   └── _Imports.razor
    │   └── LMS.Web
    │   │   ├── Components
    │   │       ├── Account
    │   │       │   ├── IdentityComponentsEndpointRouteBuilderExtensions.cs
    │   │       │   ├── IdentityNoOpEmailSender.cs
    │   │       │   ├── IdentityRedirectManager.cs
    │   │       │   ├── IdentityRevalidatingAuthenticationStateProvider.cs
    │   │       │   ├── IdentityUserAccessor.cs
    │   │       │   ├── Pages
    │   │       │   │   ├── AccessDenied.razor
    │   │       │   │   ├── ConfirmEmail.razor
    │   │       │   │   ├── ConfirmEmailChange.razor
    │   │       │   │   ├── ExternalLogin.razor
    │   │       │   │   ├── ForgotPassword.razor
    │   │       │   │   ├── ForgotPasswordConfirmation.razor
    │   │       │   │   ├── InvalidPasswordReset.razor
    │   │       │   │   ├── InvalidUser.razor
    │   │       │   │   ├── Lockout.razor
    │   │       │   │   ├── Login.razor
    │   │       │   │   ├── LoginWith2fa.razor
    │   │       │   │   ├── LoginWithRecoveryCode.razor
    │   │       │   │   ├── Manage
    │   │       │   │   │   ├── ChangePassword.razor
    │   │       │   │   │   ├── DeletePersonalData.razor
    │   │       │   │   │   ├── Disable2fa.razor
    │   │       │   │   │   ├── Email.razor
    │   │       │   │   │   ├── EnableAuthenticator.razor
    │   │       │   │   │   ├── ExternalLogins.razor
    │   │       │   │   │   ├── GenerateRecoveryCodes.razor
    │   │       │   │   │   ├── Index.razor
    │   │       │   │   │   ├── PersonalData.razor
    │   │       │   │   │   ├── ResetAuthenticator.razor
    │   │       │   │   │   ├── SetPassword.razor
    │   │       │   │   │   ├── TwoFactorAuthentication.razor
    │   │       │   │   │   └── _Imports.razor
    │   │       │   │   ├── Register.razor
    │   │       │   │   ├── RegisterConfirmation.razor
    │   │       │   │   ├── ResendEmailConfirmation.razor
    │   │       │   │   ├── ResetPassword.razor
    │   │       │   │   ├── ResetPasswordConfirmation.razor
    │   │       │   │   └── _Imports.razor
    │   │       │   └── Shared
    │   │       │   │   ├── ExternalLoginPicker.razor
    │   │       │   │   ├── ManageLayout.razor
    │   │       │   │   ├── ManageNavMenu.razor
    │   │       │   │   ├── RedirectToLogin.razor
    │   │       │   │   ├── ShowRecoveryCodes.razor
    │   │       │   │   └── StatusMessage.razor
    │   │       ├── App.razor
    │   │       ├── Layout
    │   │       │   ├── AdminLayout.razor
    │   │       │   ├── AdminLayout.razor.css
    │   │       │   ├── AdminNavMenu.razor
    │   │       │   ├── AdminNavMenu.razor.css
    │   │       │   ├── AuthLayout.razor
    │   │       │   ├── AuthLayout.razor.css
    │   │       │   ├── MainLayout.razor
    │   │       │   ├── MainLayout.razor.css
    │   │       │   ├── ManageNav.razor
    │   │       │   ├── NavMenu.razor
    │   │       │   ├── NavMenu.razor.css
    │   │       │   ├── ReportLayout.razor
    │   │       │   ├── ReportLayout.razor.css
    │   │       │   ├── ReportNavMenu.razor
    │   │       │   ├── ReportNavMenu.razor.css
    │   │       │   └── ServerLayout.razor
    │   │       ├── Pages
    │   │       │   ├── Admin
    │   │       │   │   ├── AchievementPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── Analytics.razor
    │   │       │   │   ├── AssessmentPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   ├── Index.razor
    │   │       │   │   │   └── Manage.razor
    │   │       │   │   ├── CategoryPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── CertificatePages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── CoursePages
    │   │       │   │   │   ├── CourseMaster.razor
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   ├── Index.razor
    │   │       │   │   │   └── Manage.razor
    │   │       │   │   ├── Dashboard.razor
    │   │       │   │   ├── EnrollmentPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── ForumPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── ForumPostPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── LessonPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── ModulePages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── QuestionPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   ├── Details.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   ├── TagPages
    │   │       │   │   │   ├── Create.razor
    │   │       │   │   │   └── Index.razor
    │   │       │   │   └── _Imports.razor
    │   │       │   ├── DiagnosticInfo.razor
    │   │       │   ├── Error.razor
    │   │       │   ├── Report
    │   │       │   │   ├── AssessmentPerformance.razor
    │   │       │   │   ├── Attendance.razor
    │   │       │   │   ├── CourseCompletion.razor
    │   │       │   │   ├── EnrollmentSummary.razor
    │   │       │   │   ├── EnrollmentSummaryStub.razor
    │   │       │   │   ├── EnrollmentTrends.razor
    │   │       │   │   ├── ForumActivity.razor
    │   │       │   │   ├── GradeDistribution.razor
    │   │       │   │   ├── Index.razor
    │   │       │   │   ├── Index.razor.css
    │   │       │   │   ├── LowPerformance.razor
    │   │       │   │   ├── StudentEngagement.razor
    │   │       │   │   ├── StudentInformation.razor
    │   │       │   │   ├── StudentProgress.razor
    │   │       │   │   ├── TeacherPerformance.razor
    │   │       │   │   └── _Imports.razor
    │   │       │   └── TestError.razor
    │   │       ├── Shared
    │   │       │   └── CustomErrorBoundary.razor
    │   │       └── _Imports.razor
    │   │   ├── Data
    │   │       └── ApplicationDbContext.cs
    │   │   ├── Endpoints
    │   │       ├── AchievementEndpoints.cs
    │   │       ├── AnnouncementEndpoints.cs
    │   │       ├── AssessmentEndpoints.cs
    │   │       ├── CategoryEndpoints.cs
    │   │       ├── CertificateEndpoints.cs
    │   │       ├── CourseEndpoints.cs
    │   │       ├── DropdownEndpoints.cs
    │   │       ├── EnrollmentEndpoints.cs
    │   │       ├── ForumEndpoints.cs
    │   │       ├── LeaderboardEndpoints.cs
    │   │       ├── LessonEndpoints.cs
    │   │       ├── MessageEndpoints.cs
    │   │       ├── ModuleEndpoints.cs
    │   │       ├── ProgressEndpoints.cs
    │   │       ├── TagEndpoints.cs
    │   │       └── UserEndpoints.cs
    │   │   ├── Infrastructure
    │   │       ├── AddEndpoints.cs
    │   │       ├── GlobalExceptionHandler.cs
    │   │       ├── IEndpoint.cs
    │   │       └── MapEndpoints.cs
    │   │   ├── LMS.Web.csproj
    │   │   ├── Program.cs
    │   │   ├── Properties
    │   │       ├── launchSettings.json
    │   │       ├── serviceDependencies.json
    │   │       └── serviceDependencies.local.json
    │   │   ├── Repositories
    │   │       ├── AchievementRepository.cs
    │   │       ├── AnnouncementRepository.cs
    │   │       ├── AssessmentRepository.cs
    │   │       ├── CategoryRepository.cs
    │   │       ├── CertificateRepository.cs
    │   │       ├── CourseRepository.cs
    │   │       ├── DTOs
    │   │       │   └── StudentEngagementReportDto.cs
    │   │       ├── DropdownRepository.cs
    │   │       ├── EnrollmentRepository.cs
    │   │       ├── ForumRepository.cs
    │   │       ├── LeaderboardRepository.cs
    │   │       ├── LessonRepository.cs
    │   │       ├── MessageRepository.cs
    │   │       ├── ModuleRepository.cs
    │   │       ├── ProgressRepository.cs
    │   │       ├── ReportRepository.cs
    │   │       ├── TagRepository.cs
    │   │       └── UserRepository.cs
    │   │   ├── Services
    │   │       └── ToastService.cs
    │   │   ├── appsettings.Development.json
    │   │   ├── appsettings.json
    │   │   └── wwwroot
    │   │       ├── app.css
    │   │       ├── course.png
    │   │       ├── css
    │   │           └── spinner.css
    │   │       ├── favicon.png
    │   │       └── js
    │   │           ├── dashboard-charts.js
    │   │           ├── searchable-select.js
    │   │           └── site.js
    └── LMS.sln
└── docs
    ├── Achievements.md
    ├── AdminDashboard.md
    ├── Analytics.md
    ├── Announcements.md
    ├── BlazorBootstrap.md
    ├── Certificates.md
    ├── CourseCatalog.md
    ├── CourseDetails.md
    ├── Dashboard.md
    ├── DataTableComponent.md
    ├── Forums.md
    ├── Leaderboard.md
    ├── Messages.md
    ├── MyCourses.md
    ├── Settings.md
    └── table.readme
```

_Please ensure this section is updated whenever the project structure changes._

## 1. Core Architecture: Dual-Project Setup

The application is split into two distinct projects to optimize for user experience and security:

### 1.1. `LMS.Web.Client` (User-Facing Application)

- **Framework:** Blazor WebAssembly
- **Purpose:** Provides a rich, fast, and interactive experience for end-users (students, learners). All UI interactions should be near-instantaneous.
- **Render Mode:** Must exclusively use the **Interactive WebAssembly** render mode (`@rendermode InteractiveWebAssembly`). This ensures the application runs entirely on the client's browser after the initial download, minimizing server calls for UI updates.
- **Prerendering:** **Must be disabled** for this project to prevent the UI "flicker" effect, where content appears briefly and then reloads. This ensures a smoother initial page load experience.
- **Layout:** Will use a distinct layout file (`ClientLayout.razor`) tailored for the user experience.

### 1.2. `LMS.Web` (Admin-Facing Application)

- **Framework:** Blazor Web App (Server-side)
- **Purpose:** Provides a secure and robust interface for administrators to manage the platform, including user credentials, course content, and other sensitive data. Security is the top priority.
- **Render Mode:** Will primarily use **Static Server** rendering for most pages. For components requiring user interaction (e.g., data grids, forms, dashboards), **Interactive Server** render mode (`@rendermode InteractiveServer`) will be used.
- **Layout:** Will use a separate layout file (`AdminLayout.razor`) designed for administrative tasks.

## 2. Performance and User Experience (UX)

User experience is paramount. The application must feel fast and responsive.

- **Fast Page Loads:** Both initial load and subsequent page transitions must be optimized for speed.
- **Skeleton Loaders:** On pages and components that fetch data, **MudBlazor's `MudSkeleton`** component must be used to show a loading state. This provides immediate visual feedback to the user, improving perceived performance while data is being retrieved from the server.
- **Responsiveness:** All UI components and layouts must be fully responsive and work seamlessly across various screen sizes, from mobile devices to desktops.

## 3. Styling and Theming with MudBlazor

A consistent and professional design will be implemented using the MudBlazor component library.

- **Component Library:** **MudBlazor** is the compulsory component library for this project. All UI elements (buttons, forms, tables, dialogs, etc.) should be implemented using MudBlazor components.
- **Color Palette:** The theme will use good contrasting colors to ensure readability and visual appeal. The color scheme should be modern and clean.
- **Dark & Light Themes:**
  - A `ThemeProvider` (`MudThemeProvider`) will be implemented to provide application-wide support for both **Light** and **Dark** themes.
  - A custom theme file (`CustomTheme.cs`) will define the color palettes, typography, and other design tokens for both themes.
  - The user's selected theme (Light/Dark) must be persisted. A local storage package (e.g., `Blazored.LocalStorage`) will be used to save the user's preference, so it is remembered across sessions.
- **Cohesive Design:** All custom styles and component usage must align with the overall design language established by the theme to ensure a cohesive look and feel.

## 4. State Management & Data Flow

- **Client-Side (WASM):** For managing application state on the client (e.g., user information, theme preference), a lightweight state management solution should be considered if needed.
- **API Communication:** The `LMS.Web.Client` project will communicate with the `LMS.Web` project via secure API endpoints for all data operations.

By adhering to these guidelines, the final application will be secure, performant, and provide a superior user experience.
