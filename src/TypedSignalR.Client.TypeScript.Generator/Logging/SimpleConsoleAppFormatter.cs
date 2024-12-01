using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace TypedSignalR.Client.TypeScript.Logging;

internal static class ConsoleLoggerExtensions
{
    public static ILoggingBuilder AddSimpleConsoleApp(this ILoggingBuilder builder)
    {
        builder.AddConsole(options => options.FormatterName = SimpleConsoleAppFormatter.FormatterName)
            .AddConsoleFormatter<SimpleConsoleAppFormatter, ConsoleFormatterOptions>();

        return builder;
    }
}

internal class SimpleConsoleAppFormatter : ConsoleFormatter
{
    public const string FormatterName = "SimpleConsoleApp";

    public SimpleConsoleAppFormatter() : base(FormatterName)
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        if (message is null)
        {
            return;
        }

        textWriter.WriteLine(message);
    }
}
