using System.Reflection;
using System.Runtime.CompilerServices;
using IronShield.Core.Attributes;
using IronShield.Core.Enums;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Factories;

public sealed class IronContainerFactory : IIronContainerFactory
{
    private readonly IIronBlockSerializer _serializer;
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly IIronEncryptionProfile _profile;
    private static readonly Dictionary<Type, IronBlockType> _blockTypeCache = [];

    public IronContainerFactory(IIronBlockSerializer serializer, IEncryptionProvider encryptionProvider, IIronEncryptionProfile profile)
    {
        _serializer = serializer;
        _encryptionProvider = encryptionProvider;
        _profile = profile;
    }

    public IronContainer Create(byte version, IReadOnlyCollection<IIronBlockData> data, IronCryptographyContext cryptographyContext)
    {
        List<IronBlock> blocks = [];

        blocks.Add(CreateBlock(cryptographyContext.EncryptionInfo, cryptographyContext));

        foreach(IIronBlockData blockData in data)
        {
            blocks.Add(CreateBlock(blockData,cryptographyContext));
        }

        return new IronContainer
        {
            Version = version,
            Blocks = blocks
        };
    }

    private IronBlock CreateBlock(IIronBlockData blockData, IronCryptographyContext cryptographyContext)
    {
        bool IsEncrypted = _profile.ShouldEncrypt(blockData);

        byte[] data = _serializer.Serialize(blockData);

        if (IsEncrypted)
        {
            ArgumentNullException.ThrowIfNull(cryptographyContext);
            EncryptedPayload payload = _encryptionProvider.Encrypt(data, cryptographyContext.EncryptionKey);
            data =  _serializer.Serialize(payload);
        }

        return new IronBlock
        {
            Type = ResolveBlockType(blockData),
            IsEncrypted = IsEncrypted,
            Data = data
        };
    }

    private static IronBlockType ResolveBlockType(IIronBlockData blockData)
    {
        Type modelType = blockData.GetType();

        if (_blockTypeCache.TryGetValue(modelType, out IronBlockType type))
            return type;

        IronBlockAttribute ? attribute = modelType.GetCustomAttribute<IronBlockAttribute>();

        if(attribute is null)
            throw new InvalidOperationException($"The block data type '{modelType.Name}' is not associated with an IronBlockType.");

        _blockTypeCache[modelType] = attribute.Type;

        return attribute.Type;
    }
}