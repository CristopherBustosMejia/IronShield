using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Tests.Serialization;

public sealed class JsonIronBlockSerializerInterfaceTests
{
    private readonly JsonIronBlockSerializer _serializer = new();

    [Fact]
    public void Should_Serialize_Through_Interface()
    {
        IIronBlockData data = new PublicMetadata
        {
            OriginalFileName = "test.txt",
            OriginalFileSize = 100,
            CreatedUtc = DateTimeOffset.UtcNow,
            AuthorInfo = new AuthorInfo
            {
                CreatedBy = "test",
                ApplicationName = "IronShield",
                ApplicationVersion = "1.0"
            }
        };

        byte[] bytes = _serializer.Serialize(data);

        PublicMetadata result = _serializer.Deserialize<PublicMetadata>(bytes);

        result.OriginalFileName.Should().Be("test.txt");
    }
}
