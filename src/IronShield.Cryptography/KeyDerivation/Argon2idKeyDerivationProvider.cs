using System.Text;
using Konscious.Security.Cryptography;
using IronShield.Core.Models;
using IronShield.Core.Interfaces;

namespace IronShield.Cryptography.KeyDerivation;

public sealed class Argon2idKeyDerivationProvider : IKeyDerivationProvider
{
    public String Algorithm => "Argon2id";
    private readonly IRandomProvider _randomProvider;

    public Argon2idKeyDerivationProvider(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }
    public byte[] DeriveKey(String password, IKeyDerivationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(parameters);

        if(parameters is not Argon2idParameters argon2IdParameters)
            throw new ArgumentException("Invalid key derivation parameters.",nameof(parameters));

        Argon2id argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = argon2IdParameters.Salt,
            DegreeOfParallelism = argon2IdParameters.Parallelism,
            Iterations = argon2IdParameters.Iterations,
            MemorySize = argon2IdParameters.MemorySizeKb
        };
        return argon2.GetBytes(argon2IdParameters.KeySize);
    }
    public IKeyDerivationParameters CreateParameters()
    {
        return new Argon2idParameters
        {
            Salt = _randomProvider.GetBytes(32),
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = Environment.ProcessorCount,
            KeySize = 32
        };
    }
}