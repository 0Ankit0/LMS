namespace LMS.Models.Assessment
{
    public class QuestionResponseModel
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public int? OptionId { get; set; }

        public string? TextResponse { get; set; }

        public bool IsCorrect { get; set; }

        public double PointsEarned { get; set; }

        public string? Explanation { get; set; }
    }
}
