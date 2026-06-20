using System.Text.Json;
using IronShield.Core.Interfaces;
using IronShield.Storage.Serialization.Converters;

namespace IronShield.Storage.Serialization;

public sealed class JsonIronBlockSerializer : IIronBlockSerializer
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = false,
        Converters =
        {
            new KeyDerivationParametersJsonConverter()
        }
    };
    public byte[] Serialize<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, value?.GetType() ?? typeof(T), _options);
    }
    public T Deserialize<T>(byte[] data)
    {
        T ? result = JsonSerializer.Deserialize<T>(data,_options);
        
        if(result is null)
            throw new InvalidOperationException("Failed to deserialize JSON.");

        return result;
    }
}