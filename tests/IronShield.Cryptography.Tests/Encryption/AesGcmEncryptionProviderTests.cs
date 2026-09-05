using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using IronShield.Core.Constants;
using IronShield.Core.Exceptions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Cryptography.Encryption;

namespace IronShield.Cryptography.Tests;

public sealed class AesGcmEncryptionProviderTest
{
    [Fact]
    public void Should_Encrypt_And_Decrypt_Data()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");
        IRandomProvider randomProvider = new DeterministProvider(
            Enumerable.Repeat((byte)2, CryptographyConstants.AesGcmNonceSize).ToArray());

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        EncryptedPayload payload = encryptionProvider.Encrypt(data, key);

        byte[] decrypted = encryptionProvider.Decrypt(payload, key);

        data.Should().Equal(decrypted);
    }

    [Fact]
    public void Should_Generate_Different_CipherText()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");
        IRandomProvider randomProvider = new DeterministProvider(
            Enumerable.Repeat((byte)2, CryptographyConstants.AesGcmNonceSize).ToArray());

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        EncryptedPayload payload = encryptionProvider.Encrypt(data, key);

        payload.CipherText.Should().NotEqual(data);
    }

    [Fact]
    public void Should_Store_Nonce_Parameter()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");
        byte[] nonce = Enumerable.Repeat((byte)2, CryptographyConstants.AesGcmNonceSize).ToArray();

        IRandomProvider randomProvider = new DeterministProvider(nonce);

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        EncryptedPayload payload = encryptionProvider.Encrypt(data, key);

        EncryptionParameter parameter = payload.Parameters.Single(p => p.Name == EncryptionParameterNames.Nonce);

        parameter.Value.Should().Equal(nonce);
    }

    [Fact]
    public void Should_Store_Tag_Parameter()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");
        byte[] nonce = Enumerable.Repeat((byte)2, CryptographyConstants.AesGcmNonceSize).ToArray();

        IRandomProvider randomProvider = new DeterministProvider(nonce);

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        EncryptedPayload payload = encryptionProvider.Encrypt(data, key);

        EncryptionParameter parameter = payload.Parameters.Single(p => p.Name == EncryptionParameterNames.Tag);

        parameter.Value.Should().HaveCount(CryptographyConstants.AesGcmTagSize);
    }

    [Fact]
    public void Should_Throw_When_Nonce_Is_Missing()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");

        EncryptedPayload payload = new EncryptedPayload()
        {
            CipherText = data,
            Parameters =
            [
                new EncryptionParameter
                {
                    Name = EncryptionParameterNames.Tag,
                    Value = new byte[CryptographyConstants.AesGcmTagSize]
                }
            ]
        };

        IRandomProvider randomProvider = new DeterministProvider(new byte[CryptographyConstants.AesGcmNonceSize]);

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        Action action = () => encryptionProvider.Decrypt(payload, key);

        action.Should().Throw<IronFormatException>().WithMessage("*Nonce*");
    }

    [Fact]
    public void Should_Throw_When_Tag_Is_Missing()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");

        EncryptedPayload payload = new EncryptedPayload()
        {
            CipherText = data,
            Parameters =
            [
                new EncryptionParameter
                {
                    Name = EncryptionParameterNames.Nonce,
                    Value = new byte[CryptographyConstants.AesGcmNonceSize]
                }
            ]
        };

        IRandomProvider randomProvider = new DeterministProvider(new byte[CryptographyConstants.AesGcmNonceSize]);

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        Action action = () => encryptionProvider.Decrypt(payload, key);

        action.Should().Throw<IronFormatException>().WithMessage("*Tag*");
    }

    [Fact]
    public void Should_Throw_When_CipherText_Is_Modified()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");
        IRandomProvider randomProvider = new DeterministProvider(
            Enumerable.Repeat((byte)2, CryptographyConstants.AesGcmNonceSize).ToArray());

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        EncryptedPayload payload = encryptionProvider.Encrypt(data, key);

        payload.CipherText[0] ^= 0xFF;

        Action action = () => encryptionProvider.Decrypt(payload, key);

        action.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Should_Throw_When_Key_Is_Invalid()
    {
        byte[] key = Enumerable.Repeat((byte)1, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] invalidKey = Enumerable.Repeat((byte)2, CryptographyConstants.Aes256KeySize).ToArray();
        byte[] data = Encoding.UTF8.GetBytes("IronShield Secrets");
        IRandomProvider randomProvider = new DeterministProvider(
            Enumerable.Repeat((byte)2, CryptographyConstants.AesGcmNonceSize).ToArray());

        IEncryptionProvider encryptionProvider = new AesGcmEncryptionProvider(randomProvider);

        EncryptedPayload payload = encryptionProvider.Encrypt(data, key);

        Action action = () => encryptionProvider.Decrypt(payload, invalidKey);

        action.Should().Throw<CryptographicException>();
    }
    private sealed class DeterministProvider : IRandomProvider
    {
        private readonly byte[] _bytes;

        public DeterministProvider(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte[] GetBytes(int length)
        {
            if (_bytes.Length != length)
                throw new InvalidOperationException("Determinist data length mismatch.");

            return _bytes.ToArray();
        }
        public void Fill(Span<byte> bytes)
        {
            throw new NotImplementedException("Non implemented method.");
        }
    }
}