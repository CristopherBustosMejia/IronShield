using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IEncryptionProvider
{
    EncryptedPayload Encrypt(byte[] data, byte[] key);
    byte[] Decrypt(EncryptedPayload payload, byte[] key);
}