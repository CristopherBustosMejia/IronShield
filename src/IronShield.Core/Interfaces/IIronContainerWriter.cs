using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronContainerReader
{
    IronContainer Read(byte[] data);   
}