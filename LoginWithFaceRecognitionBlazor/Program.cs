using Blazored.LocalStorage;
using LoginWithFaceRecognitionBlazor;
using LoginWithFaceRecognitionBlazor.Code;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<Fido2Service>();
builder.Services.AddBlazoredLocalStorage();
await builder.Build().RunAsync();
