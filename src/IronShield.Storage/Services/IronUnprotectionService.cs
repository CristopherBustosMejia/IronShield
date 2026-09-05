using IronShield.Core.Enums;
using IronShield.Core.Exceptions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Services;

public sealed class IronUnprotectionService : IIronUnprotectionService
{
    private readonly IIronContainerReader _reader;
    private readonly IIronBlockSerializer _serializer;
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly IKeyDerivationProvider _keyDerivationProvider;

    public IronUnprotectionService(
        IIronContainerReader reader,
        IIronBlockSerializer serializer,
        IEncryptionProvider encryptionProvider,
        IKeyDerivationProvider keyDerivationProvider)
    {
        _reader = reader;
        _serializer = serializer;
        _encryptionProvider = encryptionProvider;
        _keyDerivationProvider = keyDerivationProvider;
    }

    public UnprotectResult Unprotect(Stream input, string password)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(password);

        IronContainer container = _reader.Read(input);

        IronBlock encryptionBlock = container.Blocks.FirstOrDefault(
            b => b.Type == IronBlockType.EncryptionInfo)
            ?? throw new IronFormatException("Missing encryption info block.");

        EncryptionInfo encryptionInfo = _serializer.Deserialize<EncryptionInfo>(encryptionBlock.Data);
        byte[] key = _keyDerivationProvider.DeriveKey(password, encryptionInfo.KeyDerivationParameters);

        PublicMetadata? metadata = null;
        byte[]? data = null;

        foreach (IronBlock block in container.Blocks)
        {
            byte[] plaintext = BlockDecryptor.Decrypt(block, key, _serializer, _encryptionProvider);

            switch (block.Type)
            {
                case IronBlockType.PublicMetadata:
                    metadata = _serializer.Deserialize<PublicMetadata>(plaintext);
                    break;
                case IronBlockType.FileContent:
                    FileContent content = _serializer.Deserialize<FileContent>(plaintext);
                    data = content.Content;
                    break;
            }
        }

        return new UnprotectResult
        {
            Data = data ?? throw new IronFormatException("No file content block found."),
            Metadata = metadata
        };
    }
}
