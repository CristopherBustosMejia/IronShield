namespace IronShield.Core.Exceptions;

public sealed class IronFormatException : IronShieldException
{
    public IronFormatException()
    {
    }

    public IronFormatException(String message)
        : base(message)
    {
    }

    public IronFormatException(String message, Exception innerException)
        : base(message, innerException)
    {
    }
}