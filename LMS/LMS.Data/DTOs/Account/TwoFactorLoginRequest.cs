namespace LMS.Data.DTOs
{
    public class TwoFactorLoginRequest
    {

        public string TwoFactorCode { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public bool RememberMachine { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
