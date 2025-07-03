using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public int AssessmentId { get; set; }

        [ForeignKey("AssessmentId")]
        public virtual Assessment Assessment { get; set; } = null!;

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

        public double Points { get; set; } = 1.0;

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }

        public bool IsRequired { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public virtual ICollection<QuestionResponse> Responses { get; set; } = new List<QuestionResponse>();
    }

    public class QuestionOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        // Navigation Properties
        public virtual ICollection<QuestionResponse> Responses { get; set; } = new List<QuestionResponse>();
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
