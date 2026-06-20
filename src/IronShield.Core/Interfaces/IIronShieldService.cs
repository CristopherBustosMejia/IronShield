using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronShieldService
{
    void Protect(IDataSource source, String password, Stream output);

    UnprotectResult Unprotect(Stream input, String password);
}
