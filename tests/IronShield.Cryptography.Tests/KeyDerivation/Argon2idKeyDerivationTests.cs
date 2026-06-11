using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Cryptography.KeyDerivation;

namespace IronShield.Cryptography.Tests;

public sealed class Argon2idKeyDerivationTests
{
    [Fact]
    public void Should_Derive_Key_Expected_Size()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivation();

        Argon2idParameters parameters = new Argon2idParameters()
        {
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };

        byte[] key = provider.DeriveKey("IronFile",Enumerable.Repeat((byte)1,32).ToArray(),parameters);
        
        key.Should().HaveCount(32);
    }

    [Fact]
    public void Should_Return_Same_Key_For_Same_Input()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivation();

        Argon2idParameters parameters = new Argon2idParameters()
        {
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };
        
        byte[] salt = Enumerable.Repeat((byte)7,32).ToArray();

        byte[] key1 = provider.DeriveKey("IronFile",Enumerable.Repeat((byte)1,32).ToArray(),parameters);
        byte[] key2 = provider.DeriveKey("IronFile",Enumerable.Repeat((byte)1,32).ToArray(),parameters);

        key2.Should().Equal(key1);
    }

    [Fact]
    public void Should_Return_Different_Key_For_Different_Salt()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivation();

        Argon2idParameters parameters = new Argon2idParameters()
        {
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };
        
        byte[] salt1 = Enumerable.Repeat((byte)7,32).ToArray();
        byte[] salt2 = Enumerable.Repeat((byte)1,32).ToArray();

        byte[] key1 = provider.DeriveKey("IronFile",salt1,parameters);
        byte[] key2 = provider.DeriveKey("IronFile",salt2,parameters);

        key2.Should().NotEqual(key1);
    }

    [Fact]
    public void Should_Return_Different_Key_For_Different_Password()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivation();

        Argon2idParameters parameters = new Argon2idParameters()
        {
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };
        
        byte[] salt = Enumerable.Repeat((byte)7,32).ToArray();

        byte[] key1 = provider.DeriveKey("IronFile",salt,parameters);
        byte[] key2 = provider.DeriveKey("PlasticFile",salt,parameters);

        key2.Should().NotEqual(key1);
    }

    [Fact]
    public void Should_Return_Different_Key_For_Different_Parameters()
    {
        IKeyDerivationProvider provider = new Argon2idKeyDerivation();

        Argon2idParameters parameters1 = new Argon2idParameters()
        {
            MemorySizeKb = 65536,
            Iterations = 4,
            Parallelism = 2,
            KeySize = 32
        };
        Argon2idParameters parameters2 = new Argon2idParameters()
        {
            MemorySizeKb = 129536,
            Iterations = 2,
            Parallelism = 2,
            KeySize = 32
        };
        
        byte[] salt = Enumerable.Repeat((byte)7,32).ToArray();

        byte[] key1 = provider.DeriveKey("IronFile",salt,parameters1);
        byte[] key2 = provider.DeriveKey("IronFile",salt,parameters2);

        key2.Should().NotEqual(key1);
    }
}