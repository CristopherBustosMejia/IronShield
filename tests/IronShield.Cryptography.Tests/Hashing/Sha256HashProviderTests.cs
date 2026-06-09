using FluentAssertions;
using System.Text;
using IronShield.Cryptography.Hashing;
using System.Security.Cryptography;

namespace IronShield.Cryptography.Tests.Hashing;

public sealed class Sha256HashProviderTests
{
    [Fact]
    public void Should_Compute_Known_Hash()
    {
        Sha256HashProvider provider = new Sha256HashProvider();

        byte[] data = Encoding.UTF8.GetBytes("IronFile");

        byte[] hash = provider.ComputeHash(data);

        byte[] expected = Convert.FromHexString("1bd54863969f37e5a3f789b49344082c5cb71edfd3a55623a224a3cd5e9a5fb1");

        hash.Should().Equal(expected);
    }

    [Fact]
    public void Shoud_Return_Same_Hash_For_Same_Data()
    {
        Sha256HashProvider provider = new Sha256HashProvider();

        byte[] data = Encoding.UTF8.GetBytes("IronFile");
        byte[] hash1 = provider.ComputeHash(data);
        byte[] hash2 = provider.ComputeHash(data);

        hash1.Should().Equal(hash2);
    }

    [Fact]
    public void Should_Return_Different_Hash_For_Different_Data()
    {
        Sha256HashProvider provider = new Sha256HashProvider();

        byte[] data1 = [1, 2, 3];
        byte[] data2 = [1, 2, 4];

        byte[] hash1 = provider.ComputeHash(data1);
        byte[] hash2 = provider.ComputeHash(data2);

        hash1.Should().NotBeEqualTo(hash2);
    }

    [Fact]
    public void Should_Return_32_Byte_Hash()
    {
        Sha256HashProvider provider = new Sha256HashProvider();

        byte[] hash = provider.ComputeHash([1, 2, 3]);

        hash.Should().HaveCount(SHA256.HashSizeInBytes);
    }
}