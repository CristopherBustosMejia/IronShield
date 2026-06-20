using IronShield.Core.Enums;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Services;

public sealed class IronProtectionService : IIronProtectionService
{
    private readonly IIronBlockDataFactory _blockFactory;
    private readonly IIronCryptographyContextFactory _cryptoContextFactory;
    private readonly IIronContainerFactory _containerFactory;
    private readonly IIronContainerWriter _writer;

    public IronProtectionService(
        IIronBlockDataFactory blockFactory,
        IIronCryptographyContextFactory cryptoContextFactory,
        IIronContainerFactory containerFactory,
        IIronContainerWriter writer)
    {
        _blockFactory = blockFactory;
        _cryptoContextFactory = cryptoContextFactory;
        _containerFactory = containerFactory;
        _writer = writer;
    }

    public void Protect(IDataSource source, string password, Stream output)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(output);

        IronCryptographyContext cryptoContext = _cryptoContextFactory.Create(password);
        IReadOnlyCollection<IIronBlockData> blocks = _blockFactory.Create(source);

        IronContainer container = _containerFactory.Create(
            (byte)IronFileFormatVersion.V1,
            blocks,
            cryptoContext);

        _writer.Write(container, output);
    }
}
