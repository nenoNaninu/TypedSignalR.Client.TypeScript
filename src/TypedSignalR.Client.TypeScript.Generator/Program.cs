using Microsoft.Build.Locator;
using TypedSignalR.Client.TypeScript;

MSBuildLocator.RegisterDefaults();

var app = ConsoleApp.Create(args);

app.AddCommands<App>();

await app.RunAsync();
