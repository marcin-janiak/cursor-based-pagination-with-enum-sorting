using System.Buffers.Text;
using System.Reflection;
using System.Text;
using GreenDonut.Data.Cursors.Serializers;
using Host;

public class SomeEnumCursorKeySerializer : ICursorKeySerializer
{
    private static readonly MethodInfo _compareTo = typeof(SomeEnum).GetMethod("CompareTo", [typeof(SomeEnum)])!;
    private static readonly Encoding _encoding = Encoding.UTF8;

    public bool IsSupported(Type type)
        => type == typeof(SomeEnum);

    public MethodInfo GetCompareToMethod(Type type)
        => _compareTo;

    public object Parse(ReadOnlySpan<byte> formattedKey)
    {
        Enum.TryParse<SomeEnum>(_encoding.GetString(formattedKey), true, out var value);
        return value;
    }

    public bool TryFormat(object key, Span<byte> buffer, out int written)
    {
        return Utf8Formatter.TryFormat(((int)(SomeEnum)key), buffer, out written);
    }
}