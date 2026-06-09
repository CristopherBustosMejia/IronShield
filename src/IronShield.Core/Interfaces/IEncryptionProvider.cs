using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IEncryptionProvider
{
    EncryptedPayload Encrypt(byte[] data, String password);
    byte[] Decrypt(EncryptedPayload payload, String password);
}