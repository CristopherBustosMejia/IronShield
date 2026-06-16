using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Core.Profiles;

public sealed class DefaultIronEncryptionProfile : IIronEncryptionProfile
{
    public bool ShouldEncrypt(IIronBlockData blockData)
    {
        return blockData switch
        {
            EncryptionInfo => false,
            PublicMetadata => false,
            _ => true
        };
    }
}