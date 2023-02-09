using System.Linq;
using Microsoft.CodeAnalysis;
using Tapper.TypeMappers;

namespace TypedSignalR.Client.TypeScript.Templates;

internal static class MethodSymbolExtensions
{
    public static string WrapLambdaExpressionSyntax(this IMethodSymbol methodSymbol, ITypedSignalRTranspilationOptions options)
    {
        if (methodSymbol.Parameters.Length == 0)
        {
            return $"() => receiver.{methodSymbol.Name.Format(options.MethodStyle)}()";
        }

        var parameters = ParametersToTypeArray(methodSymbol, options);
        return $"(...args: {parameters}) => receiver.{methodSymbol.Name.Format(options.MethodStyle)}(...args)";
    }

    public static string CreateMethodString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var methodType = methodSymbol.SelectHubMethodType(specialSymbols);
        return methodType switch
        {
            HubMethodType.Unary => CreateUnaryMethodString(methodSymbol, specialSymbols, options),
            HubMethodType.ServerToClientStreaming => CreateServerToClientStreamingMethodString(methodSymbol, specialSymbols, options),
            HubMethodType.ClientToServerStreaming => CreateClientToServerStreamingMethodString(methodSymbol, specialSymbols, options),
            _ => string.Empty
        };
    }

    private static string ParametersToTypeScriptString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var parameters = methodSymbol.Parameters
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol))
            .Select(x => $"{x.Name}: {TypeMapper.MapTo(x.Type, options)}");

        return string.Join(", ", parameters);
    }

    private static string ParametersToTypeScriptArgumentString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var args = methodSymbol.Parameters
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol))
            .ToArray();

        return args.Any()
            ? $", {string.Join(", ", args.Select(x => x.Name))}"
            : string.Empty;
    }

    private static string ReturnTypeToTypeScriptString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var returnType = methodSymbol.ReturnType;
        // for sever-to-client streaming
        // IAsyncEnumerable<T>
        if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            var typeArg = (returnType as INamedTypeSymbol)!.TypeArguments[0];

            return $"IStreamResult<{TypeMapper.MapTo(typeArg, options)}>";
        }

        // Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            var typeArg = (returnType as INamedTypeSymbol)!.TypeArguments[0];

            if (SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
            {
                var typeArg2 = (typeArg as INamedTypeSymbol)!.TypeArguments[0];

                return $"IStreamResult<{TypeMapper.MapTo(typeArg2, options)}>";
            }
        }

        return TypeMapper.MapTo(methodSymbol.ReturnType, options);
    }

    private static string ParametersToTypeArray(IMethodSymbol methodSymbol, ITypedSignalRTranspilationOptions options)
    {
        var parameters = methodSymbol.Parameters.Select(x => TypeMapper.MapTo(x.Type, options));
        return $"[{string.Join(", ", parameters)}]";
    }

    private static string CreateUnaryMethodString(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var name = methodSymbol.Name.Format(options.MethodStyle);
        var parameters = methodSymbol.ParametersToTypeScriptString(specialSymbols, options);
        var returnType = methodSymbol.ReturnTypeToTypeScriptString(specialSymbols, options);
        var args = methodSymbol.ParametersToTypeScriptArgumentString(specialSymbols, options);

        return $@"
    public readonly {name} = async ({parameters}): {returnType} => {{
        return await this.connection.invoke(""{methodSymbol.Name}""{args});
    }}";
    }

    private static string CreateServerToClientStreamingMethodString(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var name = methodSymbol.Name.Format(options.MethodStyle);
        var parameters = methodSymbol.ParametersToTypeScriptString(specialSymbols, options);
        var returnType = methodSymbol.ReturnTypeToTypeScriptString(specialSymbols, options);
        var args = methodSymbol.ParametersToTypeScriptArgumentString(specialSymbols, options);

        return $@"
    public readonly {name} = ({parameters}): {returnType} => {{
        return this.connection.stream(""{methodSymbol.Name}""{args});
    }}";
    }

    private static string CreateClientToServerStreamingMethodString(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITypedSignalRTranspilationOptions options)
    {
        var name = methodSymbol.Name.Format(options.MethodStyle);
        var parameters = methodSymbol.ParametersToTypeScriptString(specialSymbols, options);
        var returnType = methodSymbol.ReturnTypeToTypeScriptString(specialSymbols, options);
        var args = methodSymbol.ParametersToTypeScriptArgumentString(specialSymbols, options);

        return $@"
    public readonly {name} = async ({parameters}): {returnType} => {{
        return await this.connection.send(""{methodSymbol.Name}""{args});
    }}";
    }
}
