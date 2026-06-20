namespace IronShield.Storage.Tests.Helpers;

public sealed class TemporaryFile : IDisposable
{
    public String Path { get; }

    public TemporaryFile(String fileName, byte[]? content = null)
    {
        String dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "IronShieldTests");
        Directory.CreateDirectory(dir);
        Path = System.IO.Path.Combine(dir, fileName);
        File.WriteAllBytes(Path, content ?? []);
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}
