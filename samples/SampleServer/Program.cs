using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SampleServer.Hub;
using TypedSignalR.Client.DevTools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseSignalRHubSpecification(); // <- Add!
app.UseSignalRHubDevelopmentUI(); // <- Add!

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chathub");

app.Run();
