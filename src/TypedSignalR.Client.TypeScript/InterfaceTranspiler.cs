using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper;
using Tapper.TypeMappers;

namespace TypedSignalR.Client.TypeScript;

internal class InterfaceTranspiler
{
    private readonly SpecialSymbols _specialSymbols;
    private readonly ITranspilationOptions _transpilationOptions;
    private readonly ILogger _logger;

    public InterfaceTranspiler(SpecialSymbols specialSymbols, ITranspilationOptions transpilationOptions, ILogger logger)
    {
        _specialSymbols = specialSymbols;
        _transpilationOptions = transpilationOptions;
        _logger = logger;
    }

    public IReadOnlyList<GeneratedSourceCode> Transpile(IEnumerable<INamedTypeSymbol> interfaceTypes)
    {
        var typeLookup = interfaceTypes.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToLookup<INamedTypeSymbol, INamespaceSymbol>(static x => x.ContainingNamespace, SymbolEqualityComparer.Default);

        var outputSourceCodeList = new List<GeneratedSourceCode>(typeLookup.Count);

        foreach (var group in typeLookup)
        {
            var codeWriter = new CodeWriter();

            AddHeader(group, ref codeWriter);

            foreach (var type in group)
            {
                _logger.Log(LogLevel.Information, "Transpile {typename}...", type.ToDisplayString());

                AddInterface(type, _specialSymbols, _transpilationOptions, ref codeWriter);
            }

            var code = codeWriter.ToString().NormalizeNewLines("\n");

            outputSourceCodeList.Add(new GeneratedSourceCode($"TypedSignalR.Client/{group.Key}.ts", code));
        }

        return outputSourceCodeList;
    }

    private void AddHeader(IGrouping<INamespaceSymbol, INamedTypeSymbol> interfaceTypes, ref CodeWriter codeWriter)
    {
        var appearTypes = interfaceTypes.SelectMany(static x => x.GetMethods())
            .SelectMany(static x => x.Parameters.Select(static y => y.Type).Concat(new[] { x.ReturnType }));

        var tapperAttributeAnnotatedTypesLookup = appearTypes
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
            codeWriter.AppendLine($"import {{ {string.Join(", ", groupingType.Select(x => x.Name))} }} from '../{groupingType.Key.ToDisplayString()}';");
        }

        codeWriter.AppendLine();
    }

    private static void AddInterface(INamedTypeSymbol interfaceSymbol, SpecialSymbols specialSymbols, ITranspilationOptions options, ref CodeWriter codeWriter)
    {
        codeWriter.AppendLine($"export type {interfaceSymbol.Name} = {{");

        foreach (var method in interfaceSymbol.GetMethods())
        {
            WriteJSDoc(method, ref codeWriter);
            codeWriter.Append($"    {method.Name.Format(options.NamingStyle)}(");
            WriteParameters(method, options, ref codeWriter);
            codeWriter.Append("): ");
            WriteReturnType(method, options, specialSymbols, ref codeWriter);
            codeWriter.AppendLine(";");
        }

        codeWriter.AppendLine("}");
        codeWriter.AppendLine();
    }

    private static void WriteJSDoc(IMethodSymbol methodSymbol, ref CodeWriter codeWriter)
    {
        codeWriter.AppendLine("    /**");

        foreach (var parameter in methodSymbol.Parameters)
        {
            codeWriter.AppendLine($"    * @param {parameter.Name} Transpied from {parameter.Type.ToDisplayString()}");
        }

        codeWriter.AppendLine($"    * @returns Transpied from {methodSymbol.ReturnType.ToDisplayString()}");
        codeWriter.AppendLine("    */");
    }

    private static void WriteParameters(IMethodSymbol methodSymbol, ITranspilationOptions options, ref CodeWriter codeWriter)
    {
        if (methodSymbol.Parameters.Length == 0)
        {
            return;
        }

        if (methodSymbol.Parameters.Length == 1)
        {
            codeWriter.Append($"{methodSymbol.Parameters[0].Name}: {TypeMapper.MapTo(methodSymbol.Parameters[0].Type, options)}");
            return;
        }

        var paramStrings = methodSymbol.Parameters.Select(x => $"{x.Name}: {TypeMapper.MapTo(x.Type, options)}");

        codeWriter.Append(string.Join(", ", paramStrings));
    }

    private static void WriteReturnType(IMethodSymbol methodSymbol, ITranspilationOptions options, SpecialSymbols specialSymbols, ref CodeWriter codeWriter)
    {
        codeWriter.Append(TypeMapper.MapTo(methodSymbol.ReturnType, options));
    }
}
