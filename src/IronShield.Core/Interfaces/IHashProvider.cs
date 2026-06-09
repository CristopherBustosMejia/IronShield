namespace IronShield.Core.Interfaces;

public interface IHashProvider
{
    byte[] ComputeHash(byte[] data);
}