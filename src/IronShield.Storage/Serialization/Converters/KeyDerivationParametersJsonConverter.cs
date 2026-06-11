using System.Text.Json;
using System.Text.Json.Serialization;
using IronShield.Core.Models;
using IronShield.Core.Interfaces;

namespace IronShield.Storage.Serialization.Converters;

public sealed class KeyDerivationParametersJsonConverter : JsonConverter<IKeyderivationParameters>
{
    public override IKeyderivationParameters? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        
        JsonElement root = document.RootElement;

        if(!root.TryGetProperty("Algorithm", out JsonElement property))
            throw new JsonException("Missing key derivation algorithm.");

        String algorithm = property.GetString() ?? throw new JsonException("Invalid key derivation algorithm.");

        return algorithm switch
        {
            "Argon2id" => JsonSerializer.Deserialize<Argon2idParameters>(root.GetRawText(),options) ?? throw new JsonException("Failed to deserialize Argon2id parameters."),
            _ => throw new JsonException($"Unsupported key derivation algorithm '{algorithm}'")
        };
    }
    public override void Write(Utf8JsonWriter writer, IKeyderivationParameters value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer,(object)value,value.GetType(),options);
    }
}