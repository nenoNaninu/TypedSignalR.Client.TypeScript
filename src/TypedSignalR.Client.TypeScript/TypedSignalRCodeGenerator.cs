using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Tapper;
using TypedSignalR.Client.TypeScript.TypeMappers;

namespace TypedSignalR.Client.TypeScript;

public class TypedSignalRCodeGenerator
{
    private readonly Compilation _compilation;
    private readonly SerializerOption _serializerOption;
    private readonly NamingStyle _namingStyle;
    private readonly ILogger _logger;

    public TypedSignalRCodeGenerator(Compilation compilation, SerializerOption serializerOption, NamingStyle namingStyle, ILogger logger)
    {
        _compilation = compilation;
        _serializerOption = serializerOption;
        _namingStyle = namingStyle;
        _logger = logger;
    }

    public IEnumerable<GeneratedSourceCode> Generate()
    {
        // preparation
        var specialSymbols = new SpecialSymbols(_compilation);

        var typeMapperProvider = new DefaultTypeMapperProvider(_compilation);
        typeMapperProvider.AddTypeMapper(new TaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new GenericTaskTypeMapper(_compilation));

        var transpilationOptions = new TranspilationOptions(typeMapperProvider, _serializerOption, _namingStyle);

        // generate index.ts + (namespace).ts

        // first, generate (namespace).ts
        var hubTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.HubAttributeSymbol);
        var receiverTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.ReceiverAttributeSymbol);

        var interfaceTranspiler = new InterfaceTranspiler(specialSymbols, transpilationOptions, _logger);
        var sources = interfaceTranspiler.Transpile(hubTypes.Concat(receiverTypes));

        // next, generate index.ts
        var apiGenerator = new ApiGenerator(specialSymbols, transpilationOptions, _logger);

        var apiCode = apiGenerator.Generate(hubTypes, receiverTypes);

        return sources.Concat(apiCode);
    }
}
