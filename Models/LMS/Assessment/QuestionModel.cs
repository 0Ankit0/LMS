using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class QuestionModel
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public int AssessmentId { get; set; }

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

        public double Points { get; set; } = 1.0;

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }

        public bool IsRequired { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<QuestionOptionModel> Options { get; set; } = new();
        public List<QuestionResponseModel> Responses { get; set; } = new();
    }

    public enum QuestionType
    {
        MultipleChoice = 1,
        TrueFalse = 2,
        ShortAnswer = 3,
        Essay = 4,
        Matching = 5,
        FillInTheBlank = 6,
        Ordering = 7
    }
}
