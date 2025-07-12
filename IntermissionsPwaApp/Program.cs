using System.Globalization;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IntermissionsPwaApp;
using IntermissionsPwaApp.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IShowService, ShowService>();
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-BE");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-BE");

await builder.Build().RunAsync();
