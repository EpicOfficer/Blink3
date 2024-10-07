namespace Blink3.API.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText, out string iv);
    string Decrypt(string cipherText, string iv);
}