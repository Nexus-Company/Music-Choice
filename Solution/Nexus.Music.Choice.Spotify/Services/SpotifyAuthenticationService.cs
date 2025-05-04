using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyAuthenticationService : IApiAuthenticationService
{
    public string Name => "Spotify";

    private readonly IHttpProvisioningService _httpProvisioningService;
    private readonly ITokenStoreService _tokenStoreService;
    private readonly SpotifyApiConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyAuthenticationService> _logger;
    private readonly IClock _clock;

    private string? accessToken = string.Empty;

    public SpotifyAuthenticationService(
        IHttpProvisioningService httpProvisioningService,
        ITokenStoreService tokenStoreService,
        HttpClient httpClient,
        IConfiguration configuration,
        IClock clock,
        ILogger<SpotifyAuthenticationService> logger)
    {
        _tokenStoreService = tokenStoreService;
        _httpProvisioningService = httpProvisioningService;
        _httpClient = httpClient;
        _logger = logger;
        _clock = clock;
        _configuration = configuration.GetSection("Spotify").Get<SpotifyApiConfiguration>()!;
    }

    public async Task<bool> CheckAuthenticationAsync()
    {
        var tokenData = await _tokenStoreService.GetTokenAsync(Name);

        if (tokenData == null || tokenData.ExpiresIn < _clock.Now || tokenData.AccessToken == null)
        {
            _logger.LogInformation("Token is either missing or expired, attempting to refresh...");
            return await TryRefreshAsync(tokenData?.RefreshToken);
        }

        accessToken = tokenData?.AccessToken;

        return true;
    }

    public async Task StartAuthenticationAsync()
    {
        _httpProvisioningService.HttpMessageReceived += TryAuthorizationAsync;
        await _httpProvisioningService.StartAsync(_configuration.RedirectUri);
    }

    private async Task<bool> TryAuthorizationAsync(HttpProvisioningMessageEventArgs e)
    {
        var code = e.Request.QueryString["code"] ?? string.Empty;

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("Authorization code is missing from the request.");
            return false;
        }

        var token = await GetTokenAsync(code, _configuration.RedirectUri);
        if (token == null)
        {
            _logger.LogWarning("Authentication failed: Invalid token response.");
            return false;
        }

        await SaveTokenAsync(token);
        await _httpProvisioningService.StopAsync();

        return true;
    }

    private async Task<bool> TryRefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No refresh token available for renewal.");
            return false;
        }

        var token = await GetTokenByRefresh(refreshToken);
        if (token == null)
        {
            _logger.LogWarning("Failed to refresh token.");
            return false;
        }

        token.Refresh ??= refreshToken;
        await SaveTokenAsync(token);

        return true;
    }

    private async Task<SpotifyTokenResponse?> GetTokenAsync(string code, string redirectUri)
    {
        return await GetTokenFromApi("authorization_code", new Dictionary<string, string>
        {
            { "code", code },
            { "redirect_uri", redirectUri }
        });
    }

    private async Task<SpotifyTokenResponse?> GetTokenByRefresh(string refreshToken)
    {
        return await GetTokenFromApi("refresh_token", new Dictionary<string, string>
        {
            { "refresh_token", refreshToken }
        });
    }

    private async Task<SpotifyTokenResponse?> GetTokenFromApi(string grantType, Dictionary<string, string> additionalParams)
    {
        var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", grantType },
            { "client_id", _configuration.ClientId },
            { "client_secret", _configuration.ClientSecret }
        }.Concat(additionalParams));

        var response = await _httpClient.PostAsync(_configuration.TokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve token. Status code: {statusCode}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SpotifyTokenResponse>(content);
    }

    private async Task SaveTokenAsync(SpotifyTokenResponse token)
    {
        accessToken = token.Token;

        var tokenData = new TokenData
        {
            RefreshToken = token.Refresh,
            ExpiresIn = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        };

        await _tokenStoreService.SaveTokenAsync(Name, tokenData);
    }

    public string? GetAcessToken()
        => accessToken;
}

public class SpotifyApiConfiguration : ApiConfiguration
{
    public string TokenEndpoint { get; set; }
}

public class SpotifyTokenResponse
{
    [JsonProperty("access_token")] public string Token { get; set; } = null!;
    [JsonProperty("token_type")] public string Type { get; set; } = null!;
    [JsonProperty("refresh_token")] public string Refresh { get; set; } = null!;
    [JsonProperty("expires_in")] public double ExpiresIn { get; set; }
}