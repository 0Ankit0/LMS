using LMS.Data;
using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class CreateQuestionModel
    {
        public string Text { get; set; } = string.Empty;

        public int AssessmentId { get; set; }

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

        public double Points { get; set; } = 1.0;

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }

        public bool IsRequired { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<CreateQuestionOptionModel> Options { get; set; } = new();
    }
}
