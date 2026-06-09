using System.Text;
using IronShield.Core.Enums;
using IronShield.Core.Models;
using IronShield.Core.Constants;
using IronShield.Core.Interfaces;

namespace IronShield.Storage.Serialization;

public sealed class IronContainerReader : IIronContainerReader
{
    public IronContainer Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using BinaryReader reader = new BinaryReader(stream,Encoding.UTF8,true);

        ValidateMagic(reader);

        byte version = reader.ReadByte();
        int blockCount = reader.ReadInt32();

        List<IronBlock> blocks = [];

        for (int i = 0; i < blockCount; i++)
        {
            blocks.Add(ReadBlock(reader));
        }
        
        return new IronContainer
        {
            Version = version,
            Blocks = blocks
        };
    }

    private void ValidateMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(IronFileConstants.MagicSize);

        if(!magic.SequenceEqual(IronFileConstants.MagicBytes))
            throw new InvalidDataException("Invalid Iron file signature.");

    }

    private IronBlock ReadBlock(BinaryReader reader)
    {
        IronBlockType type = (IronBlockType) reader.ReadByte();
        bool IsEncrypted = reader.ReadByte() == 1;
        int length = reader.ReadInt32();
        byte[] data = reader.ReadBytes(length);

        if(data.Length != length)
            throw new InvalidDataException("Unexpected end of stream.");

        return new IronBlock
        {
            Type = type,
            IsEncrypted = IsEncrypted,
            Data = data
        };
    }
}