namespace IronShield.Core.Interfaces;

public interface IRandomProvider
{
    byte[] GetBytes(int length);
    void Fill(Span<byte> buffer);
}