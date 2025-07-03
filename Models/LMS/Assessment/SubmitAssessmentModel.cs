namespace LMS.Models.Assessment
{
    public class SubmitAssessmentModel
    {
        public int AssessmentId { get; set; }

        public List<SubmitQuestionResponseModel> Responses { get; set; } = new();
    }
}
