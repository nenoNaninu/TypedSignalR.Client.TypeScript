using System;
using Tapper;

namespace TypedSignalR.Client.TypeScript;

internal static class StringExtensions
{
    public static string NormalizeNewLines(this string source, string newline)
    {
        return source.Replace("\r\n", "\n").Replace("\n", newline);
    }

    public static string NormalizeNewLines(this string source)
    {
        return source.NormalizeNewLines(Environment.NewLine);
    }

    public static string Format(this string source, NamingStyle namingStyle)
    {
        return namingStyle switch
        {
            NamingStyle.None => source,
            NamingStyle.CamelCase => $"{char.ToLower(source[0])}{source[1..]}",
            NamingStyle.PascalCase => $"{char.ToUpper(source[0])}{source[1..]}",
            _ => throw new InvalidOperationException(),
        };
    }
}
