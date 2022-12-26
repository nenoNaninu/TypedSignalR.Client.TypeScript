using Microsoft.AspNetCore.HttpLogging;
using TypedSignalR.Client.TypeScript.Tests.Server.Hubs;
using TypedSignalR.Client.TypeScript.Tests.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR()
    .AddJsonProtocol()
    .AddMessagePackProtocol();

builder.Services.AddSingleton<IDataStore, DataStore>();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.MapHub<UnaryHub>("/realtime/UnaryHub");
app.MapHub<SideEffectHub>("/realtime/SideEffectHub");
app.MapHub<ReceiverTestHub>("/realtime/ReceiverTestHub");

app.Run();
