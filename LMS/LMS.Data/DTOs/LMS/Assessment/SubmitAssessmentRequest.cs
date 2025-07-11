using System.ComponentModel.DataAnnotations;

namespace LMS.Data.DTOs
{
    public class SubmitAssessmentRequest
    {
        public int AssessmentId { get; set; }

        public List<SubmitQuestionResponseRequest> Responses { get; set; } = new();
    }
}
