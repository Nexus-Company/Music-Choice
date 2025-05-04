using System.Security.Cryptography;
using System.Text;

namespace Nexus.Music.Choice.Domain;

public static class PkceUtil
{
    public static string GenerateCodeVerifier(int length = 64)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        var random = new Random();
        var verifier = new char[length];
        for (int i = 0; i < length; i++)
        {
            verifier[i] = chars[random.Next(chars.Length)];
        }
        return new string(verifier);
    }

    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.ASCII.GetBytes(codeVerifier);
        var hash = sha256.ComputeHash(bytes);
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}