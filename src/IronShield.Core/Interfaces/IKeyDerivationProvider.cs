namespace IronShield.Core.Interfaces;

public interface IKeyDerivationProvider
{
    String Algorithm { get; }
    byte[] DeriveKey(String password, IKeyDerivationParameters parameters);
    IKeyDerivationParameters CreateParameters();
}