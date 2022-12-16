using System.Linq;
using Microsoft.CodeAnalysis;
using Tapper;
using Tapper.TypeMappers;

namespace TypedSignalR.Client.TypeScript.Templates;

internal static class MetadataExtensions
{
    public static string ParametersToTypeScriptString(this IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        var parameters = methodSymbol.Parameters.Select(x => $"{x.Name}: {TypeMapper.MapTo(x.Type, options)}");
        return string.Join(", ", parameters);
    }

    public static string ReturnTypeToTypeScriptString(this IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        return TypeMapper.MapTo(methodSymbol.ReturnType, options);
    }

    public static string WrapLambdaExpressionSyntax(this IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        if (methodSymbol.Parameters.Length == 0)
        {
            return $"() => receiver.{methodSymbol.Name.Format(options.NamingStyle)}()";
        }

        var parameters = ParametersToTypeArray(methodSymbol, options);
        return $"(...args: {parameters}) => receiver.{methodSymbol.Name.Format(options.NamingStyle)}(...args)";
    }

    private static string ParametersToTypeArray(IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        var parameters = methodSymbol.Parameters.Select(x => TypeMapper.MapTo(x.Type, options));
        return $"[{string.Join(", ", parameters)}]";
    }
}
