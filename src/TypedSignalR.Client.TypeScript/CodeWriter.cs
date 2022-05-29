using System.Runtime.CompilerServices;
using System.Text;

namespace TypedSignalR.Client.TypeScript;

public readonly struct CodeWriter
{
    private readonly StringBuilder _stringBuilder;

    public CodeWriter() : this(new StringBuilder())
    {
    }

    public CodeWriter(StringBuilder stringBuilder)
    {
        _stringBuilder = stringBuilder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value)
    {
        _stringBuilder.Append(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string value)
    {
        _stringBuilder.Append(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine()
    {
        _stringBuilder.AppendLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(string value)
    {
        _stringBuilder.AppendLine(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return _stringBuilder.ToString();
    }
}
