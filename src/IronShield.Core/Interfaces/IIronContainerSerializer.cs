using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronBlockSerializer
{
    byte[] Serialize<T>(T value);
    T Deserialize<T>(byte[] data);
}