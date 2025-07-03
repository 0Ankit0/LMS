using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class QuestionModel
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public int AssessmentId { get; set; }

        public string Type { get; set; } = string.Empty;

        public double Points { get; set; } = 1.0;

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }

        public bool IsRequired { get; set; } = true;

        public List<QuestionOptionModel> Options { get; set; } = new();

        public string? UserResponse { get; set; }

        public bool? IsCorrect { get; set; }
    }
}
