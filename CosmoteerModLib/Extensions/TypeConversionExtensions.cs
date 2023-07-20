using System.Runtime.CompilerServices;

namespace CosmoteerModLib.Extensions;

public static class TypeConversionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this bool value) => value ? 1 : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBool(this int value) => value != 0;
}