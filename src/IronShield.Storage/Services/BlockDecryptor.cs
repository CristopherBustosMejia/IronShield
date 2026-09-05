using System.Security.Cryptography;
using IronShield.Core.Exceptions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Services;

internal static class BlockDecryptor
{
    public static byte[] Decrypt(
        IronBlock block,
        byte[] key,
        IIronBlockSerializer serializer,
        IEncryptionProvider encryptionProvider)
    {
        if (!block.IsEncrypted)
            return block.Data;

        EncryptedPayload payload = serializer.Deserialize<EncryptedPayload>(block.Data);

        try
        {
            return encryptionProvider.Decrypt(payload, key);
        }
        catch (AuthenticationTagMismatchException exception)
        {
            throw new IronPasswordException("Incorrect password. Decryption failed.", exception);
        }
    }
}