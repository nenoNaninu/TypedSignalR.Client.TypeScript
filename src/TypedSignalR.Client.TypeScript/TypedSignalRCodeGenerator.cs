using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper;

namespace TypedSignalR.Client.TypeScript;

public class TypedSignalRCodeGenerator
{
    private readonly ITranspilationOptions _options;
    private readonly Compilation _compilation;

    private readonly ILogger _logger;

    public TypedSignalRCodeGenerator(
        Compilation compilation,
        ITranspilationOptions options,
        ILogger logger)
    {
        _compilation = compilation;
        _options = options;
        _logger = logger;
    }

    public IEnumerable<GeneratedSourceCode> Generate()
    {
        // preparation
        var specialSymbols = new SpecialSymbols(_compilation);

        // generate index.ts + (namespace).ts

        // first, generate (namespace).ts
        var hubTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.HubAttributeSymbols, _options.ReferencedAssembliesTranspilation);
        var receiverTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.ReceiverAttributeSymbols, _options.ReferencedAssembliesTranspilation);

        var interfaceTranspiler = new InterfaceTranspiler(specialSymbols, _options, _logger);
        var sources = interfaceTranspiler.Transpile(hubTypes.Concat(receiverTypes));

        // next, generate index.ts
        var apiGenerator = new ApiGenerator(specialSymbols, _options, _logger);

        var apiCode = apiGenerator.Generate(hubTypes, receiverTypes);

        return sources.Concat(apiCode);
    }
}
