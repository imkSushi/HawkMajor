using System.Diagnostics.CodeAnalysis;

namespace HawkMajor2.Extensions;

public static class NullableExtensions
{
    public static bool IsNull<T>(this T? nullable, [MaybeNullWhen(true)] out T value)
    {
        value = nullable;
        return nullable is null;
    }
    
    public static bool IsNotNull<T>(this T? nullable, [MaybeNullWhen(false)] out T value)
    {
        value = nullable;
        return nullable is not null;
    }
    public static bool IsNull<T1, T2>(this (T1, T2)? nullable, [MaybeNullWhen(true)] out T1 value1, [MaybeNullWhen(true)] out T2 value2)
    {
        if (nullable is null)
        {
            value1 = default;
            value2 = default;
            return true;
        }

        (value1, value2) = nullable.Value;
        return false;
    }
    
    public static bool IsNotNull<T1, T2>(this (T1, T2)? nullable, [MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2)
    {
        if (nullable is null)
        {
            value1 = default;
            value2 = default;
            return false;
        }

        (value1, value2) = nullable.Value;
        return true;
    }
    
    public static bool IsNull<T1, T2, T3>(this (T1, T2, T3)? nullable, [MaybeNullWhen(true)] out T1 value1, [MaybeNullWhen(true)] out T2 value2, [MaybeNullWhen(true)] out T3 value3)
    {
        if (nullable is null)
        {
            value1 = default;
            value2 = default;
            value3 = default;
            return true;
        }

        (value1, value2, value3) = nullable.Value;
        return false;
    }
    
    public static bool IsNotNull<T1, T2, T3>(this (T1, T2, T3)? nullable, [MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2, [MaybeNullWhen(false)] out T3 value3)
    {
        if (nullable is null)
        {
            value1 = default;
            value2 = default;
            value3 = default;
            return false;
        }

        (value1, value2, value3) = nullable.Value;
        return true;
    }
}