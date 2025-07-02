using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Api
{
    public static class AccountApi
    {
        public static IEndpointRouteBuilder MapAccountApi(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/account/login", async (
                [FromForm] LoginRequest loginRequest,
                [FromServices] SignInManager<User> signInManager,
                HttpContext context) =>
            {
                var result = await signInManager.PasswordSignInAsync(loginRequest.Email, loginRequest.Password, false, false);
                if (result.Succeeded)
                {
                    return Results.Redirect("/");
                }
                else if (result.RequiresTwoFactor)
                {
                    return Results.Redirect($"/account/loginwith2fa");
                }
                else if (result.IsLockedOut)
                {
                    return Results.Redirect($"/account/lockout");
                }
                else if (result.IsNotAllowed)
                {
                    return Results.Redirect($"/account/login?error={Uri.EscapeDataString("Your account is not allowed to sign in. Please contact support.")}");
                }
                else
                {
                    return Results.Redirect($"/account/login?error={Uri.EscapeDataString("Invalid login attempt.")}");
                }
            }).DisableAntiforgery();

            endpoints.MapPost("/api/account/loginwith2fa", async (
                [FromForm] TwoFactorLoginRequest model,
                [FromServices] SignInManager<User> signInManager,
                [FromServices] UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.TwoFactorCode, model.RememberMe, model.RememberMachine);
                if (result.Succeeded)
                {
                    return Results.Redirect(model?.ReturnUrl ?? "/");
                }
                return Results.Redirect("/account/login?error=Invalid");

            }).DisableAntiforgery();

            endpoints.MapGet("/account/externalLoginLink", async (
                HttpContext httpContext,
                [FromServices] SignInManager<User> signInManager,
                [FromServices] UserManager<User> userManager,
                [FromQuery] string provider,
                [FromQuery] string returnUrl) =>
            {
                // Clear the existing external cookie to ensure a clean login process
                await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                // Set up the callback URL for after the external provider returns
                var callbackUrl = $"/account/externalLoginLink/callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
                var userId = userManager.GetUserId(httpContext.User);
                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl, userId);

                // Initiate the external login challenge
                return Results.Challenge(properties, new[] { provider });
            });

            endpoints.MapGet("/account/externalLoginLink/callback", async (
                HttpContext httpContext,
                [FromServices] SignInManager<User> signInManager,
                [FromServices] UserManager<User> userManager,
                [FromQuery] string returnUrl) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Redirect($"{returnUrl}?statusMessage=User%20not%20found.");
                }

                var userId = await userManager.GetUserIdAsync(user);
                var info = await signInManager.GetExternalLoginInfoAsync(userId);
                if (info == null)
                {
                    return Results.Redirect($"{returnUrl}?statusMessage=Unexpected%20error%20loading%20external%20login%20info.");
                }

                var result = await userManager.AddLoginAsync(user, info);
                if (!result.Succeeded)
                {
                    return Results.Redirect($"{returnUrl}?statusMessage=The%20external%20login%20was%20not%20added.%20External%20logins%20can%20only%20be%20associated%20with%20one%20account.");
                }

                // Clear the existing external cookie to ensure a clean login process
                await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                return Results.Redirect($"{returnUrl}?statusMessage=The%20external%20login%20was%20added.");
            });

            endpoints.MapGet("/api/account/refreshsignin", async (
                HttpContext context,
                [FromServices] UserManager<User> userManager,
                [FromServices] SignInManager<User> signInManager) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                await signInManager.RefreshSignInAsync(user);
                return Results.Ok(new { message = "Sign-in refreshed." });
            }).RequireAuthorization();

            endpoints.MapGet("/api/account/logout", async (
                [FromServices] SignInManager<User> signInManager,
                HttpContext context) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect($"/account/login?error={Uri.EscapeDataString("You have been logged out. Please login to proceed.")}");
            });

            //endpoints.MapPost("/api/account/logout", async (
            //SignInManager<User> signInManager,
            //HttpContext context) =>
            //    {
            //        await signInManager.SignOutAsync();
            //        return Results.Ok(new { message = "You have been logged out." });
            //    }).RequireAuthorization();

            endpoints.MapPost("/api/account/is2famachineremembered", async (
                [FromBody] User user,
                [FromServices] UserManager<User> userManager,
                [FromServices] SignInManager<User> signInManager) =>
            {
                if (user == null)
                    return Results.NotFound();

                var isRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
                return Results.Ok(new MachineRememberedResponse { isRemembered = isRemembered });
            });

            endpoints.MapPost("/api/account/loginwithrecoverycode", async (
                [FromForm] RecoveryCodeLoginRequest model,
                [FromServices] SignInManager<User> signInManager,
                [FromServices] UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(model.RecoveryCode);
                if (result.Succeeded)
                {
                    return Results.Redirect(model?.ReturnUrl ?? "/");
                }
                else if (result.IsLockedOut)
                {
                    return Results.Redirect("/account/lockout");
                }
                else
                {
                    // Redirect back with error message
                    return Results.Redirect($"/account/loginwithrecoverycode?message={Uri.EscapeDataString("Invalid recovery code entered.")}");
                }
            }).DisableAntiforgery();

            return endpoints;
        }
    }
}