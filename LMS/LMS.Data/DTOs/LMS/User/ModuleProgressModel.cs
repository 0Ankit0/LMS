namespace LMS.Data.DTOs
{
    public class ModuleProgressModel
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }

        public string ModuleTitle { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public double ProgressPercentage { get; set; }

        public TimeSpan TimeSpent { get; set; }

        public bool IsCompleted { get; set; }

        public List<LessonProgressModel> LessonProgresses { get; set; } = new();
    }
}
