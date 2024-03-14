using System.Reflection;

namespace Backpacking.API.Utils;

[AttributeUsage(AttributeTargets.Field)]
public class StringValueAttribute : Attribute
{
    /// <summary>
    /// Holds the stringvalue for a value in an enum.
    /// </summary>
    public string StringValue { get; protected set; }

    /// <summary>
    /// Constructor used to init a StringValue Attribute
    /// </summary>
    /// <param name="value"></param>
    public StringValueAttribute(string value)
    {
        this.StringValue = value;
    }
}

public static class StringValue
{
    /// <summary>
    /// Will get the string value for a given enums value, this will
    /// only work if you assign the StringValue attribute to
    /// the items in your enum.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToStringValue(this Enum value)
    {
        Type type = value.GetType();

        FieldInfo? fieldInfo = type.GetField(value.ToString());

        StringValueAttribute[] attribs = fieldInfo?
            .GetCustomAttributes(typeof(StringValueAttribute), false)
            as StringValueAttribute[] ?? [];

        return attribs.Length > 0 ? attribs[0].StringValue : string.Empty;
    }
}

