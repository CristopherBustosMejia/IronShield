namespace IronShield.Core.Exceptions;

public class IronShieldException : Exception
{
    public IronShieldException()
    {
    }

    public IronShieldException(String message)
        : base(message)
    {
    }

    public IronShieldException(String message, Exception innerException)
        : base(message, innerException)
    {
    }
}