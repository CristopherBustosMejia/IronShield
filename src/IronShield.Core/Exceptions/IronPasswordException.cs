namespace IronShield.Core.Exceptions;

public sealed class IronPasswordException : IronShieldException
{
    public IronPasswordException()
    {
    }

    public IronPasswordException(String message)
        : base(message)
    {
    }

    public IronPasswordException(String message, Exception innerException)
        : base(message, innerException)
    {
    }
}