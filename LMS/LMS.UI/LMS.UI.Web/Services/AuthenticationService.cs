
using LMS.UI.Shared.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace LMS.UI.Web.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthenticationService(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<ClaimsPrincipal> GetUser()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User;
        }
    }
}
