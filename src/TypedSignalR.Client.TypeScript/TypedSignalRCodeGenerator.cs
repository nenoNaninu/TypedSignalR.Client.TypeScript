using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper;
using TypedSignalR.Client.TypeScript.TypeMappers;

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
        _logger = logger;

        var typeMapperProvider = new DefaultTypeMapperProvider(_compilation, options.IncludeReferencedAssemblies);
        typeMapperProvider.AddTypeMapper(new TaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new GenericTaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new AsyncEnumerableTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new ChannelReaderTypeMapper(_compilation));

        _options = new TranspilationOptions(
            typeMapperProvider,
            options.SerializerOption,
            options.NamingStyle,
            options.EnumStyle,
            options.NewLine,
            options.Indent,
            options.IncludeReferencedAssemblies
        );
    }

    public IEnumerable<GeneratedSourceCode> Generate()
    {
        // preparation
        var specialSymbols = new SpecialSymbols(_compilation);

        var typeMapperProvider = new DefaultTypeMapperProvider(_compilation, _options.IncludeReferencedAssemblies);
        typeMapperProvider.AddTypeMapper(new TaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new GenericTaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new AsyncEnumerableTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new ChannelReaderTypeMapper(_compilation));

        // generate index.ts + (namespace).ts

        // first, generate (namespace).ts
        var hubTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.HubAttributeSymbol, _options.IncludeReferencedAssemblies);
        var receiverTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.ReceiverAttributeSymbol, _options.IncludeReferencedAssemblies);

        var interfaceTranspiler = new InterfaceTranspiler(specialSymbols, _options, _logger);
        var sources = interfaceTranspiler.Transpile(hubTypes.Concat(receiverTypes));

        // next, generate index.ts
        var apiGenerator = new ApiGenerator(specialSymbols, _options, _logger);

        var apiCode = apiGenerator.Generate(hubTypes, receiverTypes);

        return sources.Concat(apiCode);
    }
}
