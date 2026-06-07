using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronContainerWriter
{
    byte[] Write(IronContainer container);   
}