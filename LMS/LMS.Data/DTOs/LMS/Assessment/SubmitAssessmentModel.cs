namespace LMS.Data.DTOs
{
    public class SubmitAssessmentModel
    {
        public int AssessmentId { get; set; }

        public List<SubmitQuestionResponseModel> Responses { get; set; } = new();
    }
}
