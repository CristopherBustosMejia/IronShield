using System.Text;
using IronShield.Core.Exceptions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Serialization;

public sealed class BinaryIronBlockSerializer : IIronBlockSerializer
{
    private const byte KeyDerivationAlgorithmArgon2id = 0x01;

    public byte[] Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            Write(writer, value);
        }

        return stream.ToArray();
    }

    public T Deserialize<T>(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var stream = new MemoryStream(data, writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        object result = typeof(T) switch
        {
            var type when type == typeof(PublicMetadata) => ReadPublicMetadata(reader),
            var type when type == typeof(FileContent) => new FileContent { Content = ReadBytes(reader) },
            var type when type == typeof(IntegrityData) => new IntegrityData { HashAlgorithm = ReadString(reader), Hash = ReadBytes(reader) },
            var type when type == typeof(EncryptionInfo) => ReadEncryptionInfo(reader),
            var type when type == typeof(EncryptedPayload) => ReadEncryptedPayload(reader),
            _ => throw new InvalidOperationException($"Binary deserialization is not supported for type '{typeof(T).Name}'.")
        };

        return (T)result;
    }

    private static void Write(BinaryWriter writer, object value)
    {
        switch (value)
        {
            case PublicMetadata metadata:
                WritePublicMetadata(writer, metadata);
                break;
            case FileContent fileContent:
                WriteBytes(writer, fileContent.Content);
                break;
            case IntegrityData integrity:
                WriteString(writer, integrity.HashAlgorithm);
                WriteBytes(writer, integrity.Hash);
                break;
            case EncryptionInfo encryptionInfo:
                WriteEncryptionInfo(writer, encryptionInfo);
                break;
            case EncryptedPayload payload:
                WriteEncryptedPayload(writer, payload);
                break;
            default:
                throw new InvalidOperationException($"Binary serialization is not supported for type '{value.GetType().Name}'.");
        }
    }

    private static void WritePublicMetadata(BinaryWriter writer, PublicMetadata metadata)
    {
        WriteString(writer, metadata.OriginalFileName);
        writer.Write(metadata.OriginalFileSize);
        WriteDateTimeOffset(writer, metadata.CreatedUtc);
        WriteString(writer, metadata.AuthorInfo.CreatedBy);
        WriteString(writer, metadata.AuthorInfo.ApplicationName);
        WriteString(writer, metadata.AuthorInfo.ApplicationVersion);
    }

    private static PublicMetadata ReadPublicMetadata(BinaryReader reader)
    {
        return new PublicMetadata
        {
            OriginalFileName = ReadString(reader),
            OriginalFileSize = reader.ReadInt64(),
            CreatedUtc = ReadDateTimeOffset(reader),
            AuthorInfo = new AuthorInfo
            {
                CreatedBy = ReadString(reader),
                ApplicationName = ReadString(reader),
                ApplicationVersion = ReadString(reader)
            }
        };
    }

    private static void WriteEncryptionInfo(BinaryWriter writer, EncryptionInfo encryptionInfo)
    {
        WriteString(writer, encryptionInfo.EncryptionAlgorithm);
        WriteKeyDerivationParameters(writer, encryptionInfo.KeyDerivationParameters);
    }

    private static EncryptionInfo ReadEncryptionInfo(BinaryReader reader)
    {
        return new EncryptionInfo
        {
            EncryptionAlgorithm = ReadString(reader),
            KeyDerivationParameters = ReadKeyDerivationParameters(reader)
        };
    }

    private static void WriteEncryptedPayload(BinaryWriter writer, EncryptedPayload payload)
    {
        WriteBytes(writer, payload.CipherText);
        writer.Write(payload.Parameters.Count);

        foreach (EncryptionParameter parameter in payload.Parameters)
        {
            WriteString(writer, parameter.Name);
            WriteBytes(writer, parameter.Value);
        }
    }

    private static EncryptedPayload ReadEncryptedPayload(BinaryReader reader)
    {
        byte[] cipherText = ReadBytes(reader);
        int parameterCount = reader.ReadInt32();

        List<EncryptionParameter> parameters = new(parameterCount);
        for (int i = 0; i < parameterCount; i++)
        {
            EncryptionParameter parameter = new()
            {
                Name = ReadString(reader),
                Value = ReadBytes(reader)
            };
            parameters.Add(parameter);
        }

        return new EncryptedPayload
        {
            CipherText = cipherText,
            Parameters = parameters
        };
    }

    private static void WriteKeyDerivationParameters(BinaryWriter writer, IKeyDerivationParameters parameters)
    {
        switch (parameters)
        {
            case Argon2idParameters argon2:
                writer.Write(KeyDerivationAlgorithmArgon2id);
                WriteBytes(writer, argon2.Salt);
                writer.Write(argon2.MemorySizeKb);
                writer.Write(argon2.Iterations);
                writer.Write(argon2.Parallelism);
                writer.Write(argon2.KeySize);
                break;
            default:
                throw new InvalidOperationException($"Key derivation algorithm '{parameters.Algorithm}' is not supported by binary serialization.");
        }
    }

    private static IKeyDerivationParameters ReadKeyDerivationParameters(BinaryReader reader)
    {
        byte algorithm = reader.ReadByte();

        switch (algorithm)
        {
            case KeyDerivationAlgorithmArgon2id:
                return new Argon2idParameters
                {
                    Salt = ReadBytes(reader),
                    MemorySizeKb = reader.ReadInt32(),
                    Iterations = reader.ReadInt32(),
                    Parallelism = reader.ReadInt32(),
                    KeySize = reader.ReadInt32()
                };
            default:
                throw new IronFormatException($"Unsupported key derivation algorithm code '{algorithm}'.");
        }
    }

    private static void WriteDateTimeOffset(BinaryWriter writer, DateTimeOffset value)
    {
        writer.Write(value.Ticks);
        writer.Write(value.Offset.Ticks);
    }

    private static DateTimeOffset ReadDateTimeOffset(BinaryReader reader)
    {
        long ticks = reader.ReadInt64();
        long offsetTicks = reader.ReadInt64();
        return new DateTimeOffset(ticks, TimeSpan.FromTicks(offsetTicks));
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static string ReadString(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        byte[] bytes = reader.ReadBytes(length);

        if (bytes.Length != length)
            throw new IronFormatException("Unexpected end of binary data.");

        return Encoding.UTF8.GetString(bytes);
    }

    private static void WriteBytes(BinaryWriter writer, byte[] value)
    {
        writer.Write(value.Length);
        writer.Write(value);
    }

    private static byte[] ReadBytes(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        byte[] bytes = reader.ReadBytes(length);

        if (bytes.Length != length)
            throw new IronFormatException("Unexpected end of binary data.");

        return bytes;
    }
}