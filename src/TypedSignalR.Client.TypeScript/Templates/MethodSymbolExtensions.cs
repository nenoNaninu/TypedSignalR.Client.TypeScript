using System.Linq;
using Microsoft.CodeAnalysis;
using Tapper;
using Tapper.TypeMappers;

namespace TypedSignalR.Client.TypeScript.Templates;

internal static class MethodSymbolExtensions
{
    public static string WrapLambdaExpressionSyntax(this IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        if (methodSymbol.Parameters.Length == 0)
        {
            return $"() => receiver.{methodSymbol.Name.Format(options.NamingStyle)}()";
        }

        var parameters = ParametersToTypeArray(methodSymbol, options);
        return $"(...args: {parameters}) => receiver.{methodSymbol.Name.Format(options.NamingStyle)}(...args)";
    }

    public static string CreateMethodString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        var methodType = SelectHubMethodType(methodSymbol, specialSymbols, options);
        return methodType switch
        {
            HubMethodType.Unary => CreateUnaryMethodString(methodSymbol, specialSymbols, options),
            HubMethodType.ServerToClientStreaming => CreateServerToClientStreamingMethodString(methodSymbol, specialSymbols, options),
            HubMethodType.ClientToServerStreaming => CreateClientToServerStreamingMethodString(methodSymbol, specialSymbols, options),
            _ => string.Empty
        };
    }

    private static string ParametersToTypeScriptString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        var parameters = methodSymbol.Parameters
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol))
            .Select(x => $"{x.Name}: {TypeMapper.MapTo(x.Type, options)}");

        return string.Join(", ", parameters);
    }

    private static string ParametersToTypeScriptArgumentString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        var args = methodSymbol.Parameters
            .Where(x => !SymbolEqualityComparer.Default.Equals(x.Type, specialSymbols.CancellationTokenSymbol))
            .ToArray();

        return args.Any()
            ? $", {string.Join(", ", args.Select(x => x.Name))}"
            : string.Empty;
    }

    private static string ReturnTypeToTypeScriptString(this IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
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

    private static string ParametersToTypeArray(IMethodSymbol methodSymbol, ITranspilationOptions options)
    {
        var parameters = methodSymbol.Parameters.Select(x => TypeMapper.MapTo(x.Type, options));
        return $"[{string.Join(", ", parameters)}]";
    }

    private static string CreateUnaryMethodString(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        var name = methodSymbol.Name.Format(options.NamingStyle);
        var parameters = methodSymbol.ParametersToTypeScriptString(specialSymbols, options);
        var returnType = methodSymbol.ReturnTypeToTypeScriptString(specialSymbols, options);
        var args = methodSymbol.ParametersToTypeScriptArgumentString(specialSymbols, options);

        return $@"
    public readonly {name} = async ({parameters}): {returnType} => {{
        return await this.connection.invoke(""{methodSymbol.Name}""{args});
    }}";
    }

    private static string CreateServerToClientStreamingMethodString(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        var name = methodSymbol.Name.Format(options.NamingStyle);
        var parameters = methodSymbol.ParametersToTypeScriptString(specialSymbols, options);
        var returnType = methodSymbol.ReturnTypeToTypeScriptString(specialSymbols, options);
        var args = methodSymbol.ParametersToTypeScriptArgumentString(specialSymbols, options);

        return $@"
    public readonly {name} = ({parameters}): {returnType} => {{
        return this.connection.stream(""{methodSymbol.Name}""{args});
    }}";
    }

    private static string CreateClientToServerStreamingMethodString(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        var name = methodSymbol.Name.Format(options.NamingStyle);
        var parameters = methodSymbol.ParametersToTypeScriptString(specialSymbols, options);
        var returnType = methodSymbol.ReturnTypeToTypeScriptString(specialSymbols, options);
        var args = methodSymbol.ParametersToTypeScriptArgumentString(specialSymbols, options);

        return $@"
    public readonly {name} = async ({parameters}): {returnType} => {{
        return await this.connection.send(""{methodSymbol.Name}""{args});
    }}";
    }

    private static HubMethodType SelectHubMethodType(IMethodSymbol methodSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options)
    {
        // sever-to-client streaming method
        //     return type : IAsyncEnumerable<T>, Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        // client-to-server streaming method
        //     parameter type : IAsyncEnumerable<T>, ChannelReader<T>
        // other
        //     ordinary method

        var returnType = methodSymbol.ReturnType;

        // sever-to-client streaming
        // IAsyncEnumerable<T>
        if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            return HubMethodType.ServerToClientStreaming;
        }

        // Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        if (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            var typeArg = (returnType as INamedTypeSymbol)!.TypeArguments[0];

            if (SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
            {
                return HubMethodType.ServerToClientStreaming;
            }
        }

        // client-to-server streaming method
        var isClientToServerStreaming = methodSymbol.Parameters
            .Any(x =>
                SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(x.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol)
            );

        return isClientToServerStreaming ? HubMethodType.ClientToServerStreaming : HubMethodType.Unary;
    }

    private enum HubMethodType
    {
        Unary,
        ServerToClientStreaming,
        ClientToServerStreaming,
    }
}
