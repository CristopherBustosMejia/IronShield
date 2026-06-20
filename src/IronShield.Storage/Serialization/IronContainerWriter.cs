using System.Text;
using IronShield.Core.Models;
using IronShield.Core.Constants;
using IronShield.Core.Interfaces;

namespace IronShield.Storage.Serialization;

public sealed class IronContainerWriter : IIronContainerWriter
{
    public void Write(IronContainer container, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(stream);

        using BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true);

        WriteHeader(writer, container);
        foreach (IronBlock block in container.Blocks)
        {
            WriteBlock(writer,block);
        }
    }
    private void WriteHeader(BinaryWriter writer, IronContainer container)
    {
        writer.Write(IronFileConstants.MagicBytes);
        writer.Write(container.Version);
        writer.Write(container.Blocks.Count);
    }
    private void WriteBlock(BinaryWriter writer, IronBlock block)
    {
        writer.Write((byte)block.Type);
        writer.Write(block.IsEncrypted ? (byte)1 : (byte)0);
        writer.Write(block.Data.Length);
        writer.Write(block.Data);
    }
}