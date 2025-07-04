namespace LMS.Models.Assessment
{
    public class SubmitQuestionResponseModel
    {
        public int QuestionId { get; set; }

        public int? OptionId { get; set; }

        public string? TextResponse { get; set; }
    }
}
