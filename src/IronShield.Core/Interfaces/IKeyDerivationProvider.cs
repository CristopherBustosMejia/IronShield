namespace IronShield.Core.Interfaces;

public interface IKeyDerivationProvider
{
    String Algorithm { get; }
    byte[] DeriveKey(String password, byte[] salt, IKeyderivationParameters parameters);
}