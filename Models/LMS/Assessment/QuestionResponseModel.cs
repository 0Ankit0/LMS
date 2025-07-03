namespace LMS.Models.Assessment
{
    public class QuestionResponseModel
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public int? SelectedOptionId { get; set; }

        public string? TextResponse { get; set; }

        public bool IsCorrect { get; set; }

        public double PointsEarned { get; set; }

        public string? CorrectAnswer { get; set; }

        public string? Explanation { get; set; }
    }
}
