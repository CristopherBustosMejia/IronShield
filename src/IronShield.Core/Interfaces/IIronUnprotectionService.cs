using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronUnprotectionService
{
    UnprotectResult Unprotect(Stream input, string password);
}
