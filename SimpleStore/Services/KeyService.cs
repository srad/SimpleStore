using System.Security.Cryptography;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Services;

/// <summary>
/// Source: https://www.camiloterevinto.com/post/simple-and-secure-api-keys-using-asp-net-core
/// </summary>
public class KeyService : IKeyService
{
    private const string Prefix = "";
    private const int NumberOfSecureBytesToGenerate = 32;
    private const int LengthOfKey = 32;

    public string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(NumberOfSecureBytesToGenerate);

        string base64String = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_");

        var keyLength = LengthOfKey - Prefix.Length;

        return Prefix + base64String[..keyLength];
    }
}