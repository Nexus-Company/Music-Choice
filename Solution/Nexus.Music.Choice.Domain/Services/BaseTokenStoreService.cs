using Nexus.Music.Choice.Domain.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nexus.Music.Choice.Domain.Services;

public abstract class BaseTokenStoreService
{
    private readonly string _storageDirectory;
    private readonly byte[] _encryptionKey;
    private readonly string _tokenFilePath;

    public BaseTokenStoreService()
    {
        _storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Music-Choice"
        );

        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }

#warning Deve sempre definir TOKEN_STORE_SECRET como variável de ambiente.
        var secret = Environment.GetEnvironmentVariable("MC_TOKEN_STORE_SECRET")
                     ?? throw new InvalidOperationException("Environment variable 'MC_TOKEN_STORE_SECRET' not set.");
        _encryptionKey = SHA256.HashData(Encoding.UTF8.GetBytes(secret));

        // Armazena todos os tokens em um único arquivo
        _tokenFilePath = Path.Combine(_storageDirectory, "magic-of-choice.tks");
    }

    public async Task SaveTokenAsync(string serviceName, TokenData token)
    {
        // Cria um objeto para armazenar o token
        var tokenClone = new TokenData
        {
            RefreshToken = token.RefreshToken,
            ExpiresIn = token.ExpiresIn
        };

        string json = JsonSerializer.Serialize(tokenClone);

        byte[] plainBytes = Encoding.UTF8.GetBytes(json);
        byte[] salt = GenerateSalt();

        // Criptografa com o salt
        byte[] encryptedBytes = Encrypt(plainBytes, _encryptionKey, salt);

        // Recupera todos os tokens do arquivo ou cria um novo dicionário
        var allTokens = await LoadAllTokensAsync();

        // Armazena o token no dicionário
        allTokens[serviceName] = new TokenFileEntry
        {
            EncryptedToken = encryptedBytes,
            Salt = salt
        };

        // Serializa os tokens para salvar no arquivo
        string allTokensJson = JsonSerializer.Serialize(allTokens);
        byte[] allTokensBytes = Encoding.UTF8.GetBytes(allTokensJson);

        await File.WriteAllBytesAsync(_tokenFilePath, allTokensBytes);
    }

    public async Task<TokenData?> GetTokenAsync(string serviceName)
    {
        var allTokens = await LoadAllTokensAsync();

        if (allTokens.TryGetValue(serviceName, out var entry))
        {
            byte[] decryptedBytes = Decrypt(entry.EncryptedToken, _encryptionKey, entry.Salt);

            string json = Encoding.UTF8.GetString(decryptedBytes);
            var token = JsonSerializer.Deserialize<TokenData>(json);

            if (token != null)
            {
                return token;
            }
        }

        return null;
    }

    public Task DeleteTokenAsync(string serviceName)
    {
        var allTokens = LoadAllTokensAsync().Result;

        if (allTokens.Remove(serviceName))
        {
            string allTokensJson = JsonSerializer.Serialize(allTokens);
            byte[] allTokensBytes = Encoding.UTF8.GetBytes(allTokensJson);

            File.WriteAllBytes(_tokenFilePath, allTokensBytes);
        }

        return Task.CompletedTask;
    }

    private async Task<Dictionary<string, TokenFileEntry>> LoadAllTokensAsync()
    {
        if (!File.Exists(_tokenFilePath))
            return new Dictionary<string, TokenFileEntry>();

        byte[] fileBytes = await File.ReadAllBytesAsync(_tokenFilePath);
        string json = Encoding.UTF8.GetString(fileBytes);

        return JsonSerializer.Deserialize<Dictionary<string, TokenFileEntry>>(json)
               ?? new Dictionary<string, TokenFileEntry>();
    }

    private static byte[] Encrypt(byte[] data, byte[] key, byte[] salt)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV(); // Gera um IV aleatório único

        // Usando o CBC (Cipher Block Chaining)
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);  // Escreve o IV no início do stream
        ms.Write(salt, 0, salt.Length);     // Escreve o salt logo após o IV

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }

        return ms.ToArray(); // Retorna os dados criptografados
    }

    private static byte[] Decrypt(byte[] data, byte[] key, byte[] salt)
    {
        using var aes = Aes.Create();
        aes.Key = key;

        // Lê o IV e o salt a partir dos dados criptografados
        byte[] iv = data.Take(aes.BlockSize / 8).ToArray();
        byte[] saltFromData = data.Skip(aes.BlockSize / 8).Take(salt.Length).ToArray();
        byte[] cipherText = data.Skip(aes.BlockSize / 8 + salt.Length).ToArray();

        // Verifica se o salt é igual ao salt armazenado
        if (!salt.SequenceEqual(saltFromData))
        {
            throw new InvalidOperationException("Salt mismatch during decryption.");
        }

        aes.IV = iv; // Atribui o IV à configuração do AES

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();

        cs.CopyTo(result); // Realiza a cópia dos dados descriptografados
        return result.ToArray(); // Retorna os dados descriptografados
    }

    private static byte[] GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[16]; // Tamanho do salt
        rng.GetBytes(salt);
        return salt; // Retorna o salt aleatório
    }

    private class TokenFileEntry
    {
        public byte[] EncryptedToken { get; set; }
        public byte[] Salt { get; set; }
    }
}
