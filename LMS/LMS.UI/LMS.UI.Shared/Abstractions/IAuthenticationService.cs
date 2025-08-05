
using System.Security.Claims;

namespace LMS.UI.Shared.Abstractions
{
    public interface IAuthenticationService
    {
        Task<ClaimsPrincipal> GetUser();
    }
}
