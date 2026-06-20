using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Sources;

public sealed class IronBlockDataFactory : IIronBlockDataFactory
{
    private readonly IHashProvider? _hashProvider;

    public IronBlockDataFactory(IHashProvider? hashProvider = null)
    {
        _hashProvider = hashProvider;
    }

    public IReadOnlyCollection<IIronBlockData> Create(IDataSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        byte[] data = ReadAllBytes(source);
        byte[]? hash = _hashProvider?.ComputeHash(data);

        var blocks = new List<IIronBlockData>(3)
        {
            new PublicMetadata
            {
                OriginalFileName = source.Name,
                OriginalFileSize = data.Length,
                CreatedUtc = DateTimeOffset.UtcNow,
                AuthorInfo = new AuthorInfo
                {
                    CreatedBy = Environment.UserName,
                    ApplicationName = "IronShield",
                    ApplicationVersion = "v0.1b"
                }
            },
            new FileContent
            {
                Content = data
            }
        };

        if (hash is not null)
        {
            blocks.Add(new IntegrityData
            {
                HashAlgorithm = _hashProvider!.Algorithm,
                Hash = hash
            });
        }

        return blocks;
    }

    private static byte[] ReadAllBytes(IDataSource source)
    {
        using Stream stream = source.OpenRead();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
