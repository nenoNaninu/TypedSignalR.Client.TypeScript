using Microsoft.CodeAnalysis;
using Tapper;

namespace TypedSignalR.Client.TypeScript;

public class TypedSignalRTranspilationOptions : TranspilationOptions, ITypedSignalRTranspilationOptions
{
    public MethodStyle MethodStyle { get; }

    public TypedSignalRTranspilationOptions(Compilation compilation,
        ITypeMapperProvider typeMapperProvider,
        SerializerOption serializerOption,
        NamingStyle namingStyle,
        EnumStyle enumStyle,
        MethodStyle methodStyle,
        NewLineOption newLineOption,
        int indent,
        bool referencedAssembliesTranspilation,
        bool enableAttributeReference) : base(
            compilation,
            typeMapperProvider,
            serializerOption,
            namingStyle,
            enumStyle,
            newLineOption,
            indent,
            referencedAssembliesTranspilation,
            enableAttributeReference)
    {
        MethodStyle = methodStyle;
    }
}
