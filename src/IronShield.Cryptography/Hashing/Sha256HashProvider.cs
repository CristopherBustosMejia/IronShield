using System.Security.Cryptography;
using IronShield.Core.Interfaces; 

namespace IronShield.Cryptography.Hashing;

public sealed class Sha256HashProvider : IHashProvider
{
    public byte[] ComputeHash(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return SHA256.HashData(data);
    }
}