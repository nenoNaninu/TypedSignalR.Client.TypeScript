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
    private readonly EnumNamingStyle _enumNamingStyle;
    private readonly bool _referencedAssembliesTranspilation;
    private readonly ILogger _logger;

    public TypedSignalRCodeGenerator(
        Compilation compilation,
        SerializerOption serializerOption,
        NamingStyle namingStyle,
        EnumNamingStyle enumNamingStyle,
        bool referencedAssembliesTranspilation,
        ILogger logger)
    {
        _compilation = compilation;
        _serializerOption = serializerOption;
        _namingStyle = namingStyle;
        _enumNamingStyle = enumNamingStyle;
        _referencedAssembliesTranspilation = referencedAssembliesTranspilation;
        _logger = logger;
    }

    public IEnumerable<GeneratedSourceCode> Generate()
    {
        // preparation
        var specialSymbols = new SpecialSymbols(_compilation);

        var typeMapperProvider = new DefaultTypeMapperProvider(_compilation, _referencedAssembliesTranspilation);
        typeMapperProvider.AddTypeMapper(new TaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new GenericTaskTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new AsyncEnumerableTypeMapper(_compilation));
        typeMapperProvider.AddTypeMapper(new ChannelReaderTypeMapper(_compilation));

        var transpilationOptions = new TranspilationOptions(typeMapperProvider, _serializerOption, _namingStyle, _enumNamingStyle);

        // generate index.ts + (namespace).ts

        // first, generate (namespace).ts
        var hubTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.HubAttributeSymbol, _referencedAssembliesTranspilation);
        var receiverTypes = _compilation.GetAttributeAnnotatedTypes(specialSymbols.ReceiverAttributeSymbol, _referencedAssembliesTranspilation);

        var interfaceTranspiler = new InterfaceTranspiler(specialSymbols, transpilationOptions, _logger);
        var sources = interfaceTranspiler.Transpile(hubTypes.Concat(receiverTypes));

        // next, generate index.ts
        var apiGenerator = new ApiGenerator(specialSymbols, transpilationOptions, _logger);

        var apiCode = apiGenerator.Generate(hubTypes, receiverTypes);

        return sources.Concat(apiCode);
    }
}
