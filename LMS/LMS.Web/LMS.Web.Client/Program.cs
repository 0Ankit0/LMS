using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using LMS.Web.Client.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();



builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<ToastService, ClientToastService>();

await builder.Build().RunAsync();
