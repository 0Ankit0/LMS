using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class CreateQuestionRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public int AssessmentId { get; set; }

        public int Type { get; set; } = 1;

        public double Points { get; set; } = 1.0;

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }

        public bool IsRequired { get; set; } = true;

        public List<CreateQuestionOptionRequest> Options { get; set; } = new();
    }
}
