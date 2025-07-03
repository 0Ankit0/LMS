namespace LMS.Models.Account
{
    public class RecoveryCodeLoginRequest
    {
        public string RecoveryCode { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
