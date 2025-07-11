using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

public class RevalidatingIdentityAuthenticationStateProvider<TUser>
    : RevalidatingServerAuthenticationStateProvider where TUser : class
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IdentityOptions _options;

    public RevalidatingIdentityAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> optionsAccessor)
        : base(loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _options = optionsAccessor.Value;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        // Only revalidate authenticated users
        var user = authenticationState.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Get the security stamp claim
        var securityStamp = user.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
        if (securityStamp == null)
        {
            return false;
        }

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TUser>>();
        var userId = userManager.GetUserId(user);
        var currentUser = await userManager.FindByIdAsync(userId);

        if (currentUser == null)
        {
            return false;
        }

        if (userManager.SupportsUserSecurityStamp)
        {
            var currentStamp = await userManager.GetSecurityStampAsync(currentUser);
            if (currentStamp != securityStamp)
            {
                return false;
            }
        }

        return true;
    }
}
