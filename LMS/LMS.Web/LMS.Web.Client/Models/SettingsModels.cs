namespace LMS.Web.Client.Models;

public class UserProfileModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Country { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}

public class PreferencesModel
{
    public string CourseDisplayMode { get; set; } = "grid";
    public string PreferredLanguage { get; set; } = "en";
    public bool DailyReminders { get; set; } = true;
    public bool DeadlineReminders { get; set; } = true;
    public bool HighContrastMode { get; set; } = false;
    public bool LargeTextMode { get; set; } = false;
}

public class NotificationModel
{
    public bool CourseUpdates { get; set; } = true;
    public bool AssignmentReminders { get; set; } = true;
    public bool ProgressReports { get; set; } = false;
    public bool AchievementNotifications { get; set; } = true;
    public bool BrowserNotifications { get; set; } = false;
    public bool MobileNotifications { get; set; } = false;
    public string EmailFrequency { get; set; } = "daily";
}

public class PrivacyModel
{
    public string ProfileVisibility { get; set; } = "students";
    public bool ShareProgress { get; set; } = true;
    public bool AllowAnalytics { get; set; } = true;
    public bool MarketingCommunications { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
}

public class PasswordChangeModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
