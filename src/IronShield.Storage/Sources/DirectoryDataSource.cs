using System.IO.Compression;
using IronShield.Core.Interfaces;

namespace IronShield.Storage.Sources;

public sealed class DirectoryDataSource : IDataSource
{
    private readonly String _directoryPath;
    private byte[]? _cachedBytes;

    public String Name => Path.GetFileName(Path.GetFullPath(_directoryPath)) + ".zip";

    public long Length => _cachedBytes?.Length ?? -1;

    public DirectoryDataSource(String directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"The specified directory was not found: '{directoryPath}'.");

        _directoryPath = directoryPath;
    }

    public Stream OpenRead()
    {
        if (_cachedBytes is null)
        {
            using var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                String fullPath = Path.GetFullPath(_directoryPath);

                foreach (String filePath in Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories))
                {
                    String entryName = Path.GetRelativePath(fullPath, filePath);
                    ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using Stream entryStream = entry.Open();
                    using Stream fileStream = File.OpenRead(filePath);
                    fileStream.CopyTo(entryStream);
                }
            }

            _cachedBytes = memoryStream.ToArray();
        }

        return new MemoryStream(_cachedBytes);
    }
}
