using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Assessment
{
    public class QuestionOptionModel
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public int QuestionId { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        public bool IsSelected { get; set; }
    }
}
