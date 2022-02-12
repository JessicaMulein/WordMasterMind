using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameEngine.Blazor;
using GameEngine.Blazor.Helpers;
using GameEngine.Blazor.Interfaces;
using GameEngine.Blazor.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args: args);
builder.RootComponents.Add<App>(selector: "#app");
builder.RootComponents.Add<HeadOutlet>(selector: "head::after");

var baseAddress = builder.HostEnvironment.BaseAddress;
builder.Services.AddHttpClient(
    name: Constants.SpaHttpClientName,
    configureClient: services
        => services.BaseAddress = new Uri(
            uriString: baseAddress));

builder.Services.AddScoped(
    implementationFactory: sp
        => sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient(name: Constants.SpaHttpClientName));

builder.Services.AddSingleton<IGameStateMachine, GameStateMachine>();

await builder
    .Build()
    .RunAsync();