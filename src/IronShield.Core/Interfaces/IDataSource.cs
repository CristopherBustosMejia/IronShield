namespace IronShield.Core.Interfaces;

public interface IDataSource
{
    String Name { get; }

    long Length { get; }

    Stream OpenRead();
}
