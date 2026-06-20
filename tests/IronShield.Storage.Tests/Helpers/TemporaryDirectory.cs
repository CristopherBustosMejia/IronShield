namespace IronShield.Storage.Tests.Helpers;

public sealed class TemporaryDirectory : IDisposable
{
    public String Path { get; }

    public TemporaryDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "IronShieldTests",
            Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
