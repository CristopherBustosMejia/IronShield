namespace IronShield.Core.Interfaces;

public interface IHashProvider
{
    String Algorithm { get; }
    byte[] ComputeHash(byte[] data);
}