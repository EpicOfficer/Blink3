using System.Security.Cryptography;
using Blink3.API.Interfaces;
using Blink3.API.Models;

namespace Blink3.API.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string? key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"{nameof(key)} cannot be null or empty", nameof(key));

        _key = Convert.FromBase64String(key);
    }

    public string Encrypt(string plainText, out string iv)
    {
        try
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            iv = Convert.ToBase64String(aes.IV);

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using StreamWriter streamWriter = new(cryptoStream);
            {
                streamWriter.Write(plainText);
                streamWriter.Flush();
                cryptoStream.FlushFinalBlock(); // Ensure all data is written
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new EncryptionException("Error occurred during encryption.", e);
        }
    }

    public string Decrypt(string cipherText, string iv)
    {
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] ivBytes = Convert.FromBase64String(iv);

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = ivBytes;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(cipherBytes);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (Exception e)
        {
            throw new EncryptionException("Error occurred during decryption.", e);
        }
    }
}