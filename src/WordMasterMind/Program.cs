using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WordMasterMind;

var builder = WebAssemblyHostBuilder.CreateDefault(args: args);
builder.RootComponents.Add<App>(selector: "#app");
builder.RootComponents.Add<HeadOutlet>(selector: "head::after");

builder.Services.AddScoped(implementationFactory: sp => new HttpClient
    {BaseAddress = new Uri(uriString: builder.HostEnvironment.BaseAddress)});

builder.Services.AddTransient(implementationFactory: sp => new HttpClient
    {
        BaseAddress = new Uri(uriString: builder.HostEnvironment.BaseAddress)
    })
    .AddBlazoredLocalStorage();

await builder.Build().RunAsync();