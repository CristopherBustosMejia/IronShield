using System.Security.Cryptography;
using IronShield.Core.Interfaces;

namespace IronShield.Cryptography.Random;

public sealed class SecureRandomProvider : IRandomProvider
{
    public byte[] GetBytes(int length)
    {
        byte[] bytes = new byte[length];
        
        RandomNumberGenerator.Fill(bytes);

        return bytes;
    }

    public void Fill(Span<byte> buffer)
    {
        RandomNumberGenerator.Fill(buffer);
    }
}