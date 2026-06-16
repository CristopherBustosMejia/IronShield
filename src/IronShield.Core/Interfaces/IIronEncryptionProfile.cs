namespace IronShield.Core.Interfaces;

public interface IIronEncryptionProfile
{
    bool ShouldEncrypt(IIronBlockData blockData);
}