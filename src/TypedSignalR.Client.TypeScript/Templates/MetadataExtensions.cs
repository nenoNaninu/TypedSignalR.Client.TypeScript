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

    public static string ToLambdaEvent(this IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        var parameters = methodSymbol.ParametersToTypeScriptString(options);
        var parametersWithoutType = string.Join(",", methodSymbol.Parameters.Select(static x => x.Name));
        return $"({parameters}) => receiver.{methodSymbol.Name.Format(options.NamingStyle)}({parametersWithoutType})";
    }
}
