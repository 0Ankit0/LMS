namespace LMS.Data.DTOs;

public class UpdateProgressRequest
{
    public int EnrollmentId { get; set; }
    public int? ModuleId { get; set; }
    public int? LessonId { get; set; }
    public double ProgressPercentage { get; set; }
    public int? TimeSpentMinutes { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public bool IsCompleted { get; set; }
}
