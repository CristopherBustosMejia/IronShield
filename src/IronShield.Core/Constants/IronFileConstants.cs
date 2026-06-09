using System.Text;

namespace IronShield.Core.Constants;

public static class IronFileConstants
{
    public const String Magic = "IRON";
    public const byte MagicSize = 4;
    public static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(Magic);
}