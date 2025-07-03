namespace LMS.Models.Assessment
{
    public class SubmitQuestionResponseModel
    {
        public int QuestionId { get; set; }

        public int? SelectedOptionId { get; set; }

        public string? TextResponse { get; set; }
    }
}
