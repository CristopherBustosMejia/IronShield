using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronContainerFactory
{
    IIronContainerFactory Create(byte[] data, String originalFileName,
        String password, AuthorInfo authorInfo);
}