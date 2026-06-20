using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronBlockDataFactory
{
    IReadOnlyCollection<IIronBlockData> Create(IDataSource source);
}
