namespace LMS.Models.User
{
    public class LessonProgressModel
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        public string LessonTitle { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TimeSpan TimeSpent { get; set; }

        public double ProgressPercentage { get; set; }

        public bool IsCompleted { get; set; }
    }
}
