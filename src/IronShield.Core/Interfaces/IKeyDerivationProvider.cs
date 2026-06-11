namespace IronShield.Core.Interfaces;

public interface IKeyDerivationProvider
{
    byte[] DeriveKey(String password, byte[] salt, IKeyderivationParameters parameters);
}