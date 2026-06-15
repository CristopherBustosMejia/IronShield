using System.Text;
using Konscious.Security.Cryptography;
using IronShield.Core.Models;
using IronShield.Core.Interfaces;

namespace IronShield.Cryptography.KeyDerivation;

public sealed class Argon2idKeyDerivation : IKeyDerivationProvider
{
    public String Algorithm => "Argon2id";
    public byte[] DeriveKey(String password, byte[] salt, IKeyderivationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(salt);
        ArgumentNullException.ThrowIfNull(parameters);

        if(parameters is not Argon2idParameters argon2IdParameters)
            throw new ArgumentException("Invalid key derivation parameters.",nameof(parameters));

        Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = argon2IdParameters.Parallelism,
            Iterations = argon2IdParameters.Iterations,
            MemorySize = argon2IdParameters.MemorySizeKb
        };
        return argon2.GetBytes(argon2IdParameters.KeySize);
    }
}