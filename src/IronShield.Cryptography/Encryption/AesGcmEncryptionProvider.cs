using System.Security.Cryptography;
using IronShield.Core.Models;
using IronShield.Core.Constants;
using IronShield.Core.Interfaces;
using IronShield.Cryptography.Extensions;

namespace IronShield.Cryptography.Encryption;

public sealed class AesGcmEncryptionProvider : IEncryptionProvider
{
    private readonly IRandomProvider _randomProvider;

    public AesGcmEncryptionProvider(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }

    public EncryptedPayload Encrypt(byte[] data, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(key);

        byte[] nonce = _randomProvider.GetBytes(CryptographyConstans.AesGcmNonceSize);
        byte[] cipherText = new byte[data.Length];
        byte[] tag = new byte[CryptographyConstans.AesGcmTagSize];

        using AesGcm aes = new AesGcm(key,CryptographyConstans.AesGcmTagSize);

        aes.Encrypt(nonce,data,cipherText,tag);

        return new EncryptedPayload
        {
            CipherText = cipherText,
            Parameters =
            [
                new EncryptionParameter
                {
                    Name = EncryptionParameterNames.Nonce,
                    Value = nonce
                },
                new EncryptionParameter
                {
                    Name = EncryptionParameterNames.Tag,
                    Value = tag
                }
            ]
        };
    }
    
    public byte[] Decrypt(EncryptedPayload payload, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(key);

        byte[] nonce = payload.Parameters.GetRequiredValue(EncryptionParameterNames.Nonce);
        byte[] tag = payload.Parameters.GetRequiredValue(EncryptionParameterNames.Tag);
        byte[] plainText = new byte[payload.CipherText.Length];

        using AesGcm aes = new AesGcm(key, CryptographyConstans.AesGcmTagSize);

        aes.Decrypt(nonce,payload.CipherText,tag,plainText);

        return plainText;
    }
}