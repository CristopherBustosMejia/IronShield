using IronShield.Core.Interfaces;

namespace IronShield.Storage.Sources;

public sealed class FileDataSource : IDataSource
{
    private readonly String _filePath;
    private long? _cachedLength;

    public String Name => Path.GetFileName(_filePath);

    public long Length => _cachedLength ??= new FileInfo(_filePath).Length;

    public FileDataSource(String filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file was not found.", filePath);

        _filePath = filePath;
    }

    public Stream OpenRead()
    {
        return File.OpenRead(_filePath);
    }
}
