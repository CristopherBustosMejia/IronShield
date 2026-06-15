using IronShield.Core.Enums;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Storage.Factories;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Factories;

public sealed class IronContainerFactory : IIronContainerFactory
{
    private IronBlock CreateBlock<T>(IronBlockType type, T value, byte[] ? encryptionKey)
    {
        
    }
    private static bool ShouldEncrypt(IronBlockType type)
    {
        return type switch
        {
            IronBlockType.PublicMetadata => false,
            IronBlockType.EncryptionInfo => false,

            IronBlockType.IntegrityData => true,
            IronBlockType.FileContent => true,

            _ => true
        };
    }
}