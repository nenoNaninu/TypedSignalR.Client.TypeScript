using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypedSignalR.Client.TypeScript;
using Xunit;

namespace TypedSignalR.Client.TypeScript.Tests;

public class RoslynExtensionsTests
{
    [Fact]
    public void GetAttributeAnnotatedTypes_DeduplicatesPartialInterfaces()
    {
        const string source = """
using System.Threading.Tasks;
using TypedSignalR.Client;

namespace TypedSignalR.Client
{
    public sealed class HubAttribute : Attribute { }
}

[Hub]
public partial interface IClientHub
{
    Task Join(int clientId);
}

public partial interface IClientHub
{
    Task Leave(int clientId);
}
""";

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var hubAttributeSymbol = compilation.GetTypeByMetadataName("TypedSignalR.Client.HubAttribute");
        Assert.NotNull(hubAttributeSymbol);

        var annotatedTypes = compilation.GetAttributeAnnotatedTypes(hubAttributeSymbol!, includeReferencedAssemblies: false);

        var clientHub = Assert.Single(annotatedTypes);
        Assert.Equal("IClientHub", clientHub.Name);
        Assert.Equal(2, clientHub.GetMembers().OfType<IMethodSymbol>().Count());
    }
}
