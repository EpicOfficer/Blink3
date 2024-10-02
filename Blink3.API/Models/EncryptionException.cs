namespace Blink3.API.Models;

public class EncryptionException(string message, Exception innerException) : Exception(message, innerException);