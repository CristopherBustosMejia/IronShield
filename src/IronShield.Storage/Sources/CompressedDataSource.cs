using System.IO.Compression;
using IronShield.Core.Interfaces;

namespace IronShield.Storage.Sources;

public sealed class CompressedDataSource : IDataSource
{
    private readonly IDataSource _inner;
    private byte[]? _cachedBytes;

    public String Name => _inner.Name + ".gz";

    public long Length => _cachedBytes?.Length ?? -1;

    public CompressedDataSource(IDataSource inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    public Stream OpenRead()
    {
        if (_cachedBytes is null)
        {
            using Stream input = _inner.OpenRead();
            using var output = new MemoryStream();

            using (var gzip = new GZipStream(output, CompressionLevel.Optimal, true))
            {
                input.CopyTo(gzip);
            }

            _cachedBytes = output.ToArray();
        }

        return new MemoryStream(_cachedBytes);
    }
}
