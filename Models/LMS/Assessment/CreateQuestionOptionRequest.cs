using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class CreateQuestionOptionRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }
    }
}
