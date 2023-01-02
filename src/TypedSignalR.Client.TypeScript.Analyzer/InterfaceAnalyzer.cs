using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TypedSignalR.Client.TypeScript.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InterfaceAnalyzer : DiagnosticAnalyzer
{
    // Analysis Items
    // 1. Method parameter is
    //     - primitive support type
    //     - annotated TranspilationSourceAttribute
    // 2. Hub method return type must be Task, Task<T>, IAsyncEnumerable<T>, Task<IAsyncEnumerable<T>>, or Task<ChannelReader<T>>
    //     Task, Task<T> :
    //         - Ordinary hub method
    //     IAsyncEnumerable<T>, Task<IAsyncEnumerable<T>>, or Task<ChannelReader<T> :
    //         - server-to-client streaming method
    // 3. Receiver method return type must be Task or Task<T>
    //     Task :
    //         - Ordinary receiver method
    //     Task<T> :
    //         - Client results
    // 4. HubAttribute and ReceiverAttribute can not apply generic interface

    private static readonly DiagnosticDescriptor AnnotationRule = new(
        id: "TSRTS001",
        title: "Must apply TranspilationSourceAttribute",
        messageFormat: "Apply the TranspilationSourceAttribute to the {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Must apply TranspilationSourceAttribute.");

    private static readonly DiagnosticDescriptor HubAttributeAnnotationRule = new(
        id: "TSRTS002",
        title: "It is prohibited to apply the HubAttribute to generic types",
        messageFormat: "It is prohibited to apply the HubAttribute to generic types. {1} is generic type.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "It is prohibited to apply the HubAttribute to generic types.");

    private static readonly DiagnosticDescriptor ReceiverAttributeAnnotationRule = new(
        id: "TSRTS003",
        title: "It is prohibited to apply the ReceiverAttribute to generic types",
        messageFormat: "It is prohibited to apply the ReceiverAttribute to generic types. {1} is generic type.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "It is prohibited to apply the ReceiverAttribute to generic types.");

    private static readonly DiagnosticDescriptor UnsupportedTypeRule = new(
        id: "TSRTS004",
        title: "Unsupported type",
        messageFormat: "{0} is unsupported type",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Unsupported type.");

    public static readonly DiagnosticDescriptor HubMethodReturnTypeRule = new(
        id: "TSRTS005",
        title: "The return type of methods in the interface must be Task or Task<T>",
        messageFormat: "[The return type of methods in the interface used for hub proxy must be Task or Task<T>] The return type of {0} is not Task or Task<T>",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of methods in the interface used for hub proxy must be Task or Task<T>.");

    public static readonly DiagnosticDescriptor ReceiverMethodReturnTypeRule = new(
        id: "TSRTS006",
        title: "The return type of methods in the interface must be Task",
        messageFormat: "[The return type of methods in the interface used for the receiver must be Task] The return type of {0} is not Task",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The return type of methods in the interface must be Task.");

    private static readonly Type[] SupportTypes = new[]
    {
        // Primitive
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort),
        typeof(object),
        typeof(string),
        typeof(Uri),
        typeof(Guid),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(Nullable<>),
        // Collection
        typeof(Array),
        typeof(ArraySegment<>),
        typeof(List<>),
        typeof(LinkedList<>),
        typeof(Queue<>),
        typeof(Stack<>),
        typeof(HashSet<>),
        typeof(IEnumerable<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyList<>),
        typeof(ICollection<>),
        typeof(IList<>),
        typeof(ISet<>),
        // Dictionary
        typeof(Dictionary<,>),
        typeof(IDictionary<,>),
        typeof(IReadOnlyDictionary<,>)
        // streaming
        //typeof(IAsyncEnumerable<T>), // cannot use in netstandard2.0
        //typeof(ChannelReader<T>) // cannot use in netstandard2.0
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
    => ImmutableArray.Create(AnnotationRule, HubAttributeAnnotationRule, ReceiverAttributeAnnotationRule, UnsupportedTypeRule, HubMethodReturnTypeRule, ReceiverMethodReturnTypeRule);

    public override void Initialize(AnalysisContext context)
    {
        //Debug.WriteLine("InterfaceAnalyzer is Initialize");
        //Console.WriteLine("InterfaceAnalyzer is Initialize");

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        var hubAttributeSymbol = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.HubAttribute");
        var receiverAttributeSymbol = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.ReceiverAttribute");
        var transpilationSourceAttributeSymbol = context.Compilation.GetTypeByMetadataName("Tapper.TranspilationSourceAttribute");

        if (hubAttributeSymbol is null || receiverAttributeSymbol is null || transpilationSourceAttributeSymbol is null)
        {
            return;
        }

        var isHubType = namedTypeSymbol.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, hubAttributeSymbol));

        var isReceiverType = namedTypeSymbol.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, receiverAttributeSymbol));

        if (!(isHubType || isReceiverType))
        {
            return;
        }

        var specialSymbols = new SpecialSymbols(context.Compilation);

        var supportTypeSymbols = SupportTypes
            .Select(x => context.Compilation.GetTypeByMetadataName(x.FullName!)!)
            .ToArray();

        if (isHubType)
        {
            AnalyzeHubInterface(context, namedTypeSymbol, supportTypeSymbols, transpilationSourceAttributeSymbol, specialSymbols);
        }

        if (isReceiverType)
        {
            AnalyzeReceiverInterface(context, namedTypeSymbol, supportTypeSymbols, transpilationSourceAttributeSymbol, specialSymbols);
        }
    }

    private static void AnalyzeHubInterface(
        SymbolAnalysisContext context,
        INamedTypeSymbol namedTypeSymbol,
        INamedTypeSymbol[] supportTypeSymbols,
        INamedTypeSymbol transpilationSourceAttributeSymbol,
        SpecialSymbols specialSymbols)
    {
        if (namedTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HubAttributeAnnotationRule, namedTypeSymbol.Locations[0], namedTypeSymbol.ToDisplayString()));
            return;
        }

        foreach (var method in namedTypeSymbol.GetMethods())
        {
            ValidateHubReturnType(context, method, supportTypeSymbols, transpilationSourceAttributeSymbol, specialSymbols);
            ValidateHubParameterType(context, method, supportTypeSymbols, transpilationSourceAttributeSymbol, specialSymbols);
        }
    }

    private static void AnalyzeReceiverInterface(
        SymbolAnalysisContext context,
        INamedTypeSymbol namedTypeSymbol,
        INamedTypeSymbol[] supportTypeSymbols,
        INamedTypeSymbol transpilationSourceAttributeSymbol,
        SpecialSymbols specialSymbols)
    {
        if (namedTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ReceiverAttributeAnnotationRule, namedTypeSymbol.Locations[0], namedTypeSymbol.ToDisplayString()));
            return;
        }

        foreach (var method in namedTypeSymbol.GetMethods())
        {
            // Return type must be Task
            ValidateReceiverReturnType(context, method, supportTypeSymbols, transpilationSourceAttributeSymbol, specialSymbols);

            foreach (var parameter in method.Parameters)
            {
                ValidateType(context, parameter.Type, parameter.Locations[0], supportTypeSymbols, transpilationSourceAttributeSymbol);
            }
        }
    }

    private static void ValidateType(
        SymbolAnalysisContext context,
        ITypeSymbol typeSymbol,
        Location location,
        INamedTypeSymbol[] supportTypeSymbols,
        INamedTypeSymbol transpilationSourceAttribute)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            var sourceType = namedTypeSymbol.IsGenericType
               ? namedTypeSymbol.OriginalDefinition
               : namedTypeSymbol;

            if (supportTypeSymbols.Contains(sourceType, SymbolEqualityComparer.Default))
            {
                if (namedTypeSymbol.IsGenericType)
                {
                    foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                    {
                        ValidateType(context, typeArgument, location, supportTypeSymbols, transpilationSourceAttribute);
                    }
                }

                return;
            }

            if (namedTypeSymbol.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, transpilationSourceAttribute)))
            {
                return;
            }

            if (namedTypeSymbol.ContainingNamespace.ToDisplayString().StartsWith("System"))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    UnsupportedTypeRule, location, sourceType.ToDisplayString()));
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                AnnotationRule, location, namedTypeSymbol.ToDisplayString()));

            return;
        }

        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            ValidateType(context, arrayTypeSymbol.ElementType, location, supportTypeSymbols, transpilationSourceAttribute);
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            UnsupportedTypeRule, location, typeSymbol.ToDisplayString()));
    }

    /// <summary>
    /// Return type must be Task, Task&lt;T&gt;, IAsyncEnumerable&lt;T&gt;, Task&lt;IAsyncEnumerable&lt;T&gt;&gt;, or Task&lt;ChannelReader&lt;T&gt;&gt;
    /// </summary>
    private static void ValidateHubReturnType(
        SymbolAnalysisContext context,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol[] supportTypeSymbols,
        INamedTypeSymbol transpilationSourceAttribute,
        SpecialSymbols specialSymbols)
    {
        var location = methodSymbol.Locations[0];

        if (methodSymbol.ReturnType is not INamedTypeSymbol namedReturnTypeSymbol)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HubMethodReturnTypeRule, location, methodSymbol.ReturnType.ToDisplayString()));
            return;
        }

        // Task, IAsyncEnumerable<T>
        if (SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol, specialSymbols.TaskSymbol)
            || SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol))
        {
            return;
        }

        // Task<T>,  Task<IAsyncEnumerable<T>>, Task<ChannelReader<T>>
        if (SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol.OriginalDefinition, specialSymbols.GenericTaskSymbol))
        {
            // Task<T> -> T
            var typeArg = namedReturnTypeSymbol.TypeArguments[0];

            var featuredType = SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                || SymbolEqualityComparer.Default.Equals(typeArg.OriginalDefinition, specialSymbols.ChannelReaderSymbol)
                ? (typeArg as INamedTypeSymbol)!.TypeArguments[0] // IAsyncEnumerable<T>, ChannelReader<T> -> T
                : typeArg;

            ValidateType(context, featuredType, location, supportTypeSymbols, transpilationSourceAttribute);
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            HubMethodReturnTypeRule, location, methodSymbol.ToDisplayString()));
    }

    private static void ValidateHubParameterType(
        SymbolAnalysisContext context,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol[] supportTypeSymbols,
        INamedTypeSymbol transpilationSourceAttribute,
        SpecialSymbols specialSymbols)
    {
        var methodType = methodSymbol.SelectHubMethodType(specialSymbols);

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (methodType == HubMethodType.ServerToClientStreaming)
            {
                if (SymbolEqualityComparer.Default.Equals(parameter.Type, specialSymbols.CancellationTokenSymbol))
                {
                    continue;
                }
            }

            if (methodType == HubMethodType.ClientToServerStreaming)
            {
                if (SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, specialSymbols.AsyncEnumerableSymbol)
                    || SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, specialSymbols.ChannelReaderSymbol))
                {
                    var typeArg = (parameter.Type as INamedTypeSymbol)!.TypeArguments[0];
                    ValidateType(context, typeArg, parameter.Locations[0], supportTypeSymbols, transpilationSourceAttribute);

                    continue;
                }
            }

            ValidateType(context, parameter.Type, parameter.Locations[0], supportTypeSymbols, transpilationSourceAttribute);
        }
    }

    private static void ValidateReceiverReturnType(
        SymbolAnalysisContext context,
        IMethodSymbol methodSymbol,
        INamedTypeSymbol[] supportTypeSymbols,
        INamedTypeSymbol transpilationSourceAttribute,
        SpecialSymbols specialSymbols)
    {
        var location = methodSymbol.Locations[0];

        if (methodSymbol.ReturnType is not INamedTypeSymbol namedReturnTypeSymbol)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ReceiverMethodReturnTypeRule, location, methodSymbol.ReturnType.ToDisplayString()));
            return;
        }

        // Task
        if (SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol, specialSymbols.TaskSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            ReceiverMethodReturnTypeRule, location, methodSymbol.ReturnType.ToDisplayString()));
    }
}
