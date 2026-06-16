using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronContainerFactory
{
    IronContainer Create(byte version, IReadOnlyCollection<IIronBlockData> data, IronCryptographyContext cryptographyContext);
}