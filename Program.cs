using System.Configuration;
using LMS.Api;
using LMS.Components;
using LMS.Config;
using LMS.Data;
using LMS.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddIdentityAndDb(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();

// Add LMS services
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AssessmentService>();
builder.Services.AddScoped<ProgressService>();

var app = builder.Build();

// Ensure database is created and apply any pending migrations
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    try
    {
        logger.LogInformation("Attempting to initialize database...");
        context.Database.EnsureCreated();
        logger.LogInformation("Database initialization completed successfully.");
        // Optionally run migrations if you prefer that approach
        // context.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
        // In development, you might want to throw to see the error immediately
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // In development, use the exception handler but also log to console
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAccountApi();

app.Run();

