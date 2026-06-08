using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronContainerWriter
{
    void Write(IronContainer container, Stream stream);   
}