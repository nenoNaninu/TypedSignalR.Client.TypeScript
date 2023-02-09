using System;
using Tapper;

namespace TypedSignalR.Client.TypeScript;

internal static class StringExtensions
{
    public static string NormalizeNewLines(this string text, string newline)
    {
        return text.Replace("\r\n", "\n").Replace("\n", newline);
    }

    public static string NormalizeNewLines(this string text)
    {
        return text.NormalizeNewLines(Environment.NewLine);
    }

    public static string Format(this string text, NamingStyle namingStyle)
    {
        return namingStyle switch
        {
            NamingStyle.None => text,
            NamingStyle.CamelCase => ToCamel(text),
            NamingStyle.PascalCase => ToPascal(text),
            _ => throw new InvalidOperationException(),
        };
    }

    public static string Format(this string text, MethodStyle methodStyle)
    {
        return methodStyle switch
        {
            MethodStyle.None => text,
            MethodStyle.CamelCase => ToCamel(text),
            MethodStyle.PascalCase => ToPascal(text),
            _ => throw new InvalidOperationException(),
        };
    }

    private static string ToCamel(string text) => $"{char.ToLower(text[0])}{text[1..]}";

    private static string ToPascal(string text) => $"{char.ToUpper(text[0])}{text[1..]}";
}
