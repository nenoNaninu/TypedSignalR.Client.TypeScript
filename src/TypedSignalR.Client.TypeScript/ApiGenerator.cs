using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper;
using TypedSignalR.Client.TypeScript.CodeAnalysis;
using TypedSignalR.Client.TypeScript.Templates;

namespace TypedSignalR.Client.TypeScript;

internal class ApiGenerator
{
    private readonly ITranspilationOptions _transpilationOptions;
    private readonly ILogger _logger;
    private readonly SpecialSymbols _specialSymbols;

    public ApiGenerator(SpecialSymbols specialSymbols, ITranspilationOptions transpilationOptions, ILogger logger)
    {
        _specialSymbols = specialSymbols;
        _transpilationOptions = transpilationOptions;
        _logger = logger;
    }

    public IReadOnlyList<GeneratedSourceCode> Generate(IReadOnlyList<INamedTypeSymbol> hubTypes, IReadOnlyList<INamedTypeSymbol> receiverTypes)
    {
        _logger.Log(LogLevel.Information, "Generate TypedSignalR.Client.TypeScript API script...");

        var template = new ApiTemplate()
        {
            Header = GenerateHeader(hubTypes, receiverTypes),
            HubTypes = hubTypes.Select(static x => new TypeMetadata(x)).ToArray(),
            ReceiverTypes = receiverTypes.Select(static x => new TypeMetadata(x)).ToArray(),
            SpecialSymbols = _specialSymbols,
            TranspilationOptions = _transpilationOptions,
        };

        return new[] { new GeneratedSourceCode("TypedSignalR.Client/index.ts", template.TransformText().NormalizeNewLines("\n")) };
    }

    private string GenerateHeader(IReadOnlyList<INamedTypeSymbol> hubTypes, IReadOnlyList<INamedTypeSymbol> receiverTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"import { HubConnection, IStreamResult, Subject } from '@microsoft/signalr';");

        var interfaceLookup = hubTypes.Concat(receiverTypes)
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToLookup<INamedTypeSymbol, INamespaceSymbol>(static x => x.ContainingNamespace, SymbolEqualityComparer.Default);

        foreach (var group in interfaceLookup)
        {
            sb.AppendLine($"import {{ {string.Join(", ", group.Select(x => x.Name))} }} from './{group.Key.ToDisplayString()}';");
        }

        var hubParametersAndReturnTypes = hubTypes
            .SelectMany(static x => x.GetMethods())
            .SelectMany(x =>
                x.Parameters
                    .Select(y => y.Type.GetFeaturedType(_specialSymbols))
                    .Concat(new[] { x.ReturnType.GetFeaturedType(_specialSymbols) })
            );

        var receiverParameterTypes = receiverTypes
            .SelectMany(static x => x.GetMethods())
            .SelectMany(x =>
                x.Parameters
                    .Select(y => y.Type.GetFeaturedType(_specialSymbols))
                    .Concat(new[] { x.ReturnType.GetFeaturedType(_specialSymbols) })
            );

        var tapperAttributeAnnotatedTypesLookup = hubParametersAndReturnTypes.Concat(receiverParameterTypes)
            .OfType<INamedTypeSymbol>()
            .Where(x => x.IsAttributeAnnotated(_specialSymbols.TranspilationSourceAttributeSymbol))
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToLookup<INamedTypeSymbol, INamespaceSymbol>(static x => x.ContainingNamespace, SymbolEqualityComparer.Default);

        foreach (var groupingType in tapperAttributeAnnotatedTypesLookup)
        {
            // Be careful about the directory hierarchy.
            // Tapper generates a file named (namespace).ts directly under the specified directory(e.g. generated/HogeNamespace.ts).
            // TypedSignalR.Client.TypeScript creates a directory named TypedSignalR.Client in the specified directory
            // and generates TypeScript files there. (e.g. generated/TypedSignalR.Client/index.ts)
            // Therefore, in order to refer to the TypeScript file created by Tapper, we have to specify the directory one level up.
            sb.AppendLine($"import {{ {string.Join(", ", groupingType.Select(x => x.Name))} }} from '../{groupingType.Key.ToDisplayString()}';");
        }

        return sb.ToString();
    }
}
