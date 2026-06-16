using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Cryptography.KeyDerivation;

namespace IronShield.Cryptography.Tests;

public sealed class Argon2idKeyDerivationTests
{
    [Fact]
    public void Should_Derive_Key_Expected_Size()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivationProvider(new DeterministProvider());

        Argon2idParameters parameters = new Argon2idParameters()
        {
            Salt = Enumerable.Repeat((byte)1,32).ToArray(),
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };

        byte[] key = provider.DeriveKey("IronFile",parameters);
        
        key.Should().HaveCount(32);
    }

    [Fact]
    public void Should_Return_Same_Key_For_Same_Input()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivationProvider(new DeterministProvider());

        Argon2idParameters parameters = new Argon2idParameters()
        {
            Salt = Enumerable.Repeat((byte)1,32).ToArray(),
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };
        
        byte[] salt = Enumerable.Repeat((byte)7,32).ToArray();

        byte[] key1 = provider.DeriveKey("IronFile",parameters);
        byte[] key2 = provider.DeriveKey("IronFile",parameters);

        key2.Should().Equal(key1);
    }

    [Fact]
    public void Should_Return_Different_Key_For_Different_Password()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivationProvider(new DeterministProvider());

        Argon2idParameters parameters = new Argon2idParameters()
        {
            Salt = Enumerable.Repeat((byte)7,32).ToArray(),
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };

        byte[] key1 = provider.DeriveKey("IronFile",parameters);
        byte[] key2 = provider.DeriveKey("PlasticFile",parameters);

        key2.Should().NotEqual(key1);
    }

    [Fact]
    public void Should_Return_Different_Key_For_Different_Parameters()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivationProvider(new DeterministProvider());

        Argon2idParameters parameters1 = new Argon2idParameters()
        {
            Salt = Enumerable.Repeat((byte)7,32).ToArray(),
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };
        Argon2idParameters parameters2 = new Argon2idParameters()
        {
            Salt = Enumerable.Repeat((byte)7,32).ToArray(),
            MemorySizeKb = 129536,
            Iterations = 2,
            Parallelism = 2,
            KeySize = 32
        };

        byte[] key1 = provider.DeriveKey("IronFile",parameters1);
        byte[] key2 = provider.DeriveKey("IronFile",parameters2);

        key2.Should().NotEqual(key1);
    }

    private sealed class DeterministProvider : IRandomProvider
    {
        public byte[] GetBytes(int length)
        {
            throw new NotImplementedException();
        }
        public void Fill(Span<byte> bytes)
        {
            throw new NotImplementedException();
        }
    }
}