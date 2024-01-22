#if NET8_0_OR_GREATER
using AspNetCore.SignalR.OpenTelemetry;
#endif
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.HttpLogging;
using TypedSignalR.Client.TypeScript.Tests.Server.Hubs;
using TypedSignalR.Client.TypeScript.Tests.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR()
#if NET8_0_OR_GREATER
    .AddHubInstrumentation()
#endif
    .AddJsonProtocol()
   // dotnet tsrts --project path/to/Project.csproj --output generated --serializer MessagePack --naming-style none --enum name
   // .AddMessagePackProtocol(); // default
   // Configuration to serialize an enum as a value instead of a string.
   // dotnet tsrts --project path/to/Project.csproj --output generated --serializer MessagePack --naming-style none
   .AddMessagePackProtocol(options =>
   {
       options.SerializerOptions = MessagePackSerializerOptions.Standard
           .WithResolver(ContractlessStandardResolver.Instance)
           .WithSecurity(MessagePackSecurity.UntrustedData);
   });

builder.Services.AddSingleton<IDataStore, DataStore>();

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.MapHub<UnaryHub>("/hubs/UnaryHub");
app.MapHub<SideEffectHub>("/hubs/SideEffectHub");
app.MapHub<ReceiverTestHub>("/hubs/ReceiverTestHub");
app.MapHub<StreamingHub>("/hubs/StreamingHub");
app.MapHub<ClientResultsTestHub>("/hubs/ClientResultsTestHub");
app.MapHub<NestedTypeHub>("/hubs/NestedTypeHub");
app.MapHub<InheritHub>("/hubs/InheritHub");

app.Run();
