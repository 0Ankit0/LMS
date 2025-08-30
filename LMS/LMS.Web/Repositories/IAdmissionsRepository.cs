using LMS.Data.DTOs.UserManagement;

namespace LMS.Repositories
{
    public interface IAdmissionsRepository
    {
        Task<List<AdmissionApplicationDto>> GetAllApplicationsAsync();
        Task<AdmissionApplicationDto?> GetApplicationByIdAsync(int id);
        Task<List<ApplicationDocumentDto>?> GetApplicationDocumentsAsync(int applicationId);
        Task<List<ProgramDto>> GetAvailableProgramsAsync();
        Task<int> SubmitApplicationAsync(AdmissionApplicationDto application);
        Task UpdateApplicationStatusAsync(int applicationId, string status, string? notes = null);
        Task MakeDecisionAsync(int applicationId, string decision, string? notes = null);
        Task<bool> UploadDocumentAsync(int applicationId, string fileName, string filePath, string documentType);
    }
}
