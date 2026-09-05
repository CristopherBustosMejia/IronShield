using System.Security.Cryptography;
using IronShield.Core.Enums;
using IronShield.Core.Exceptions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Services;

public sealed class IronIntegrityVerificationService : IIronIntegrityVerificationService
{
    private readonly IIronContainerReader _reader;
    private readonly IIronBlockSerializer _serializer;
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly IKeyDerivationProvider _keyDerivationProvider;
    private readonly IHashProvider _hashProvider;

    public IronIntegrityVerificationService(
        IIronContainerReader reader,
        IIronBlockSerializer serializer,
        IEncryptionProvider encryptionProvider,
        IKeyDerivationProvider keyDerivationProvider,
        IHashProvider hashProvider)
    {
        _reader = reader;
        _serializer = serializer;
        _encryptionProvider = encryptionProvider;
        _keyDerivationProvider = keyDerivationProvider;
        _hashProvider = hashProvider;
    }

    public IntegrityVerificationResult Verify(Stream input, String password)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(password);

        IronContainer container = _reader.Read(input);

        IronBlock encryptionBlock = container.Blocks.FirstOrDefault(
            b => b.Type == IronBlockType.EncryptionInfo)
            ?? throw new IronFormatException("Missing encryption info block.");

        EncryptionInfo encryptionInfo = _serializer.Deserialize<EncryptionInfo>(encryptionBlock.Data);
        byte[] key = _keyDerivationProvider.DeriveKey(password, encryptionInfo.KeyDerivationParameters);

        byte[]? content = null;
        IntegrityData? integrity = null;

        foreach (IronBlock block in container.Blocks)
        {
            byte[] plaintext = BlockDecryptor.Decrypt(block, key, _serializer, _encryptionProvider);

            switch (block.Type)
            {
                case IronBlockType.FileContent:
                    FileContent fileContent = _serializer.Deserialize<FileContent>(plaintext);
                    content = fileContent.Content;
                    break;
                case IronBlockType.IntegrityData:
                    integrity = _serializer.Deserialize<IntegrityData>(plaintext);
                    break;
            }
        }

        if (integrity is null)
            return new IntegrityVerificationResult
            {
                IsAvailable = false,
                IsValid = false
            };

        if (content is null)
            throw new IronFormatException("No file content block found.");

        if (integrity.HashAlgorithm != _hashProvider.Algorithm)
            return new IntegrityVerificationResult
            {
                IsAvailable = true,
                IsValid = false,
                HashAlgorithm = integrity.HashAlgorithm
            };

        byte[] actualHash = _hashProvider.ComputeHash(content);
        bool valid = CryptographicOperations.FixedTimeEquals(integrity.Hash, actualHash);

        return new IntegrityVerificationResult
        {
            IsAvailable = true,
            IsValid = valid,
            HashAlgorithm = integrity.HashAlgorithm
        };
    }
}