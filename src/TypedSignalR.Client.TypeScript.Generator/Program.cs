using Cocona;
using Microsoft.Build.Locator;
using TypedSignalR.Client.TypeScript;
using TypedSignalR.Client.TypeScript.Logging;

MSBuildLocator.RegisterDefaults();

var builder = CoconaApp.CreateBuilder();

builder.Logging.AddSimpleConsoleApp();

var app = builder.Build();

app.AddCommands<App>();

await app.RunAsync();
