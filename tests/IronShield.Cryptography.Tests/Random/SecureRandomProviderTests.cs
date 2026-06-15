using FluentAssertions;
using IronShield.Cryptography.Random;

namespace IronShield.Cryptography.Tests;

public sealed class SecureRandomProviderTests
{
    [Fact]
    public void Should_Return_Requested_Number_Of_Bytes()
    {
        SecureRandomProvider provider = new SecureRandomProvider();

        byte[] bytes = provider.GetBytes(32);

        bytes.Should().HaveCount(32);
    }

    [Fact]
    public void Should_Return_Different_Values()
    {
        SecureRandomProvider provider = new SecureRandomProvider();

        byte[] first = provider.GetBytes(32);
        byte[] second = provider.GetBytes(32);

        second.Should().NotEqual(first);
    }
}