using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WordMasterMind.Blazor;
using WordMasterMind.Blazor.Interfaces;
using WordMasterMind.Blazor.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args: args);
builder.RootComponents.Add<App>(selector: "#app");
builder.RootComponents.Add<HeadOutlet>(selector: "head::after");

builder.Services.AddScoped(implementationFactory: sp
    => new HttpClient
    {
        BaseAddress = new Uri(
            uriString: builder.HostEnvironment.BaseAddress),
    });

// Register our own injectables
builder.Services.AddSingleton<IGameStateMachine, GameStateMachine>();

await builder.Build().RunAsync();