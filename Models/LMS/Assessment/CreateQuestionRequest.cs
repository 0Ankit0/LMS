using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class CreateQuestionRequest
    {
        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public int AssessmentId { get; set; }

        public int Type { get; set; } = 1; // 1 = MultipleChoice, 2 = TrueFalse, etc.

        [Range(0.1, 100)]
        public double Points { get; set; } = 1.0;

        public int OrderIndex { get; set; }

        [StringLength(2000)]
        public string? Explanation { get; set; }

        public bool IsRequired { get; set; } = true;

        public List<CreateQuestionOptionRequest> Options { get; set; } = new();
    }
}
