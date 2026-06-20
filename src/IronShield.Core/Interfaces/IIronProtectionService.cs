using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronProtectionService
{
    void Protect(IDataSource source, string password, Stream output);
}
