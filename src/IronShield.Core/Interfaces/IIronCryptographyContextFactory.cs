using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronCryptographyContextFactory
{
    IronCryptographyContext Create(String password);
}