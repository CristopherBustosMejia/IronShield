using IronShield.Core.Exceptions;
using IronShield.Core.Models;

namespace IronShield.Cryptography.Extensions;

internal static class EncryptionParameterExtensions
{
    public static byte[] GetRequiredValue(
        this IReadOnlyCollection<EncryptionParameter> parameters, String name)
    {
        return parameters.FirstOrDefault(p => p.Name == name)?.Value 
            ?? throw new IronFormatException($"Missing encryption parameter '{name}'.");
    }
}