using System.Diagnostics.CodeAnalysis;

namespace Backpacking.API.Utils;

public class Guard
{
    public static bool IsNotNull([NotNullWhen(true)] object value)
    {
        return value is not null;
    }

    public static bool IsNotNullOrEmpty([NotNullWhen(true)] string? value)
    {
        return string.IsNullOrEmpty(value) == false;
    }

    public static bool IsNotEmpty([NotNullWhen(true)] Guid value)
    {
        return Guid.Empty.Equals(value) == false;
    }

    public static bool IsNotEqual(object value1, object value2)
    {
        return Equals(value1, value2) == false;
    }

    public static bool IsEqual(object value1, object value2)
    {
        return Equals(value1, value2);
    }

    public static bool IsBefore(DateOnly start, DateOnly end)
    {
        return start < end;
    }
}
