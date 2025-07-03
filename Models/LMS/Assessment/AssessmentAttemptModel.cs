namespace LMS.Models.Assessment
{
    public class AssessmentAttemptModel
    {
        public int Id { get; set; }

        public int EnrollmentId { get; set; }

        public int AssessmentId { get; set; }

        public string AssessmentTitle { get; set; } = string.Empty;

        public int AttemptNumber { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public double? Score { get; set; }

        public double? Percentage { get; set; }

        public bool IsPassed { get; set; }

        public TimeSpan? TimeTaken { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<QuestionResponseModel> Responses { get; set; } = new();
    }
}
