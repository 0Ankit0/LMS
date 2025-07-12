using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class QuestionOptionModel
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public int QuestionId { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        public List<QuestionResponseModel> Responses { get; set; } = new();
    }
}
