using IronShield.Core.Enums;

namespace IronShield.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class IronBlockAttribute : Attribute
{
    public IronBlockAttribute(IronBlockType type)
    {
        Type = type;
    }

    public IronBlockType Type { get; }
}