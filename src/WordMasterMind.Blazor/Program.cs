using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WordMasterMind.Blazor;
using WordMasterMind.Blazor.Helpers;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Blazor.Models;

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