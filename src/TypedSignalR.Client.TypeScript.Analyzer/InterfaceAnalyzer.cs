using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TypedSignalR.Client.TypeScript.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InterfaceAnalyzer : DiagnosticAnalyzer
{
    // Analysis Items
    // 1. Method parameter is primitive support type or annotated TranspilationSourceAttribute
    // 2. Hub method return type must be Task or Task<T>
    // 3. Receiver method return type must be Task
    // 4. HubAttribute and ReceiverAttribute can not apply generic interface

    private static readonly DiagnosticDescriptor AnnotationRule = new DiagnosticDescriptor(
        id: "TSRTS001",
        title: "Must apply TranspilationSourceAttribute",
        messageFormat: "Apply the TranspilationSourceAttribute to the {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Must apply TranspilationSourceAttribute.");

    private static readonly DiagnosticDescriptor HubAttributeAnnotationRule = new DiagnosticDescriptor(
        id: "TSRTS002",
        title: "It is prohibited to apply the HubAttribute to generic types",
        messageFormat: "It is prohibited to apply the HubAttribute to generic types. {1} is generic type.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "It is prohibited to apply the HubAttribute to generic types.");

    private static readonly DiagnosticDescriptor ReceiverAttributeAnnotationRule = new DiagnosticDescriptor(
        id: "TSRTS003",
        title: "It is prohibited to apply the ReceiverAttribute to generic types",
        messageFormat: "It is prohibited to apply the ReceiverAttribute to generic types. {1} is generic type.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "It is prohibited to apply the ReceiverAttribute to generic types.");

    private static readonly DiagnosticDescriptor UnsupportedTypeRule = new DiagnosticDescriptor(
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
        var hubAttributeSymbol = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.TypeScript.HubAttribute");
        var receiverAttributeSymbol = context.Compilation.GetTypeByMetadataName("TypedSignalR.Client.TypeScript.ReceiverAttribute");
        var transpilationSourceAttributeSymbol = context.Compilation.GetTypeByMetadataName("Tapper.TranspilationSourceAttribute");
        var taskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
        var genericTaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")!;

        if (hubAttributeSymbol is null || receiverAttributeSymbol is null || transpilationSourceAttributeSymbol is null)
        {
            return;
        }

        var supportTypeSymbols = SupportTypes
            .Select(x => context.Compilation.GetTypeByMetadataName(x.FullName)!)
            .ToArray();

        if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        var isHubType = namedTypeSymbol.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, hubAttributeSymbol));

        var isReceiverType = namedTypeSymbol.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, receiverAttributeSymbol));

        if (isHubType)
        {
            AnalyzeHubInterface(context, namedTypeSymbol, supportTypeSymbols, transpilationSourceAttributeSymbol, taskSymbol, genericTaskSymbol);
        }

        if (isReceiverType)
        {
            AnalyzeReceiverInterface(context, namedTypeSymbol, supportTypeSymbols, transpilationSourceAttributeSymbol, taskSymbol);
        }
    }

    private static void AnalyzeHubInterface(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol[] supportTypeSymbols, INamedTypeSymbol transpilationSourceAttributeSymbol, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericTaskSymbol)
    {
        if (namedTypeSymbol.IsGenericType)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HubAttributeAnnotationRule, namedTypeSymbol.Locations[0], namedTypeSymbol.ToDisplayString()));
            return;
        }

        foreach (var method in namedTypeSymbol.GetMethods())
        {
            // Return type must be Task or Task<T>
            ValidateHubReturnType(context, method, supportTypeSymbols, transpilationSourceAttributeSymbol, taskSymbol, genericTaskSymbol);

            foreach (var parameter in method.Parameters)
            {
                ValidateType(context, parameter.Type, parameter.Locations[0], supportTypeSymbols, transpilationSourceAttributeSymbol);
            }
        }
    }

    private static void AnalyzeReceiverInterface(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol[] supportTypeSymbols, INamedTypeSymbol transpilationSourceAttributeSymbol, INamedTypeSymbol taskSymbol)
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
            ValidateReceiverReturnType(context, method, supportTypeSymbols, transpilationSourceAttributeSymbol, taskSymbol);

            foreach (var parameter in method.Parameters)
            {
                ValidateType(context, parameter.Type, parameter.Locations[0], supportTypeSymbols, transpilationSourceAttributeSymbol);
            }
        }
    }

    private static void ValidateType(SymbolAnalysisContext context, ITypeSymbol typeSymbol, Location location, INamedTypeSymbol[] supportTypeSymbols, INamedTypeSymbol transpilationSourceAttribute)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            var sourceType = namedTypeSymbol.IsGenericType
               ? namedTypeSymbol.ConstructedFrom
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
        return;
    }

    private static void ValidateHubReturnType(SymbolAnalysisContext context, IMethodSymbol methodSymbol, INamedTypeSymbol[] supportTypeSymbols, INamedTypeSymbol transpilationSourceAttribute, INamedTypeSymbol taskSymbol, INamedTypeSymbol genericTaskSymbol)
    {
        var location = methodSymbol.Locations[0];

        if (methodSymbol.ReturnType is not INamedTypeSymbol namedReturnTypeSymbol)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HubMethodReturnTypeRule, location, methodSymbol.ReturnType.ToDisplayString()));
            return;
        }

        if (!namedReturnTypeSymbol.IsGenericType)
        {
            // Task
            if (SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol, taskSymbol))
            {
                return;
            }
        }

        // Task<T>
        if (!SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol.ConstructedFrom, genericTaskSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                HubMethodReturnTypeRule, location, methodSymbol.ToDisplayString()));
        }

        // Validate type arguments of Task<T>
        ValidateType(context, namedReturnTypeSymbol.TypeArguments[0]!, location, supportTypeSymbols, transpilationSourceAttribute);
    }

    private static void ValidateReceiverReturnType(SymbolAnalysisContext context, IMethodSymbol methodSymbol, INamedTypeSymbol[] supportTypeSymbols, INamedTypeSymbol transpilationSourceAttribute, INamedTypeSymbol taskSymbol)
    {
        var location = methodSymbol.Locations[0];

        if (methodSymbol.ReturnType is not INamedTypeSymbol namedReturnTypeSymbol)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ReceiverMethodReturnTypeRule, location, methodSymbol.ReturnType.ToDisplayString()));
            return;
        }

        // Task
        if (SymbolEqualityComparer.Default.Equals(namedReturnTypeSymbol, taskSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            ReceiverMethodReturnTypeRule, location, methodSymbol.ReturnType.ToDisplayString()));
    }
}
