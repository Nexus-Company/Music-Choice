﻿using Nexus.Music.Choice.Domain.Services;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal class TokenStoreService : ITokenStoreService
{
    private readonly string _storageDirectory;
    private readonly byte[] _encryptionKey;
    private readonly string _tokenFilePath;
    private readonly ConcurrentDictionary<string, string> _accessTokenCache = new();

    public TokenStoreService()
    {
        _storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MusicChoiceTokens"
        );

        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }

#warning Deve sempre definir TOKEN_STORE_SECRET como variável de ambiente.
        var secret = Environment.GetEnvironmentVariable("TOKEN_STORE_SECRET")
                     ?? throw new InvalidOperationException("Environment variable 'TOKEN_STORE_SECRET' not set.");

        using var sha = SHA256.Create();
        _encryptionKey = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));

        // Armazena todos os tokens em um único arquivo
        _tokenFilePath = Path.Combine(_storageDirectory, "all_tokens.token");
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

        // Armazena o AccessToken apenas em memória
        _accessTokenCache[serviceName] = token.AccessToken;
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
                // Tenta pegar o AccessToken da memória
                if (_accessTokenCache.TryGetValue(serviceName, out var accessToken))
                {
                    token.AccessToken = accessToken;
                }

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

        _accessTokenCache.TryRemove(serviceName, out _);

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
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        ms.Write(salt, 0, salt.Length); // Escreve o salt no início

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }

        return ms.ToArray();
    }

    private static byte[] Decrypt(byte[] data, byte[] key, byte[] salt)
    {
        using var aes = Aes.Create();
        aes.Key = key;

        byte[] iv = data.Take(aes.BlockSize / 8).ToArray();
        byte[] saltFromData = data.Skip(aes.BlockSize / 8).Take(salt.Length).ToArray();
        byte[] cipherText = data.Skip(aes.BlockSize / 8 + salt.Length).ToArray();

        if (!salt.SequenceEqual(saltFromData))
        {
            throw new InvalidOperationException("Salt mismatch during decryption.");
        }

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();

        cs.CopyTo(result);
        return result.ToArray();
    }

    private static byte[] GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[16]; // Tamanho do salt
        rng.GetBytes(salt);
        return salt;
    }

    private class TokenFileEntry
    {
        public byte[] EncryptedToken { get; set; }
        public byte[] Salt { get; set; }
    }
}