using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexus.Music.Choice.Domain;
using Nexus.Music.Choice.Domain.Models;
using Nexus.Music.Choice.Domain.Services.EventArgs;
using Nexus.Music.Choice.Domain.Services.Interfaces;
using Nexus.Music.Choice.Spotify.Services.Interfaces;
using System.Web;

namespace Nexus.Music.Choice.Spotify.Services;

public class SpotifyAuthenticationService : ISpotifyApiAuthenticationService, IDisposable
{
    public string Name => "Spotify";

    private readonly IHttpProvisioningService _httpProvisioningService;
    private readonly ITokenStoreService _tokenStoreService;
    private readonly SpotifyApiConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyAuthenticationService> _logger;
    private readonly IClock _clock;
    private readonly string codeVerifier = PkceUtil.GenerateCodeVerifier();

    public SpotifyAuthenticationService(
        IHttpProvisioningServiceFactory httpProvisioningService,
        ISpotifyTokenStoreService tokenStoreService,
        HttpClient httpClient,
        IConfiguration configuration,
        IClock clock,
        ILogger<SpotifyAuthenticationService> logger)
    {
        _tokenStoreService = tokenStoreService;
        _httpProvisioningService = httpProvisioningService.GetOrCreate<SpotifyAuthenticationService>();
        _httpClient = httpClient;
        _logger = logger;
        _clock = clock;
        _configuration = configuration.GetSection("Spotify").Get<SpotifyApiConfiguration>()!;
    }

    public async Task<bool> CheckAuthenticationAsync()
    {
        var tokenData = await _tokenStoreService.GetTokenAsync();

        if (!CheckAuthorizationIsValid(tokenData))
        {
            _logger.LogInformation("Token is either missing or expired, attempting to refresh...");
            return await TryRefreshAsync(tokenData?.RefreshToken);
        }

        return true;
    }

    public async Task StartAuthenticationAsync()
    {
        _httpProvisioningService.HttpMessageReceived += TryAuthorizationAsync;

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["client_id"] = _configuration.ClientId;
        query["response_type"] = "code";
        query["redirect_uri"] = _configuration.RedirectUri;
        query["code_challenge_method"] = "S256";
        query["code_challenge"] = PkceUtil.GenerateCodeChallenge(codeVerifier);
        query["scope"] = string.Join(" ", _configuration.Scopes);

        string authorizeUrl = $"https://accounts.spotify.com/authorize?{query}";

        Environment.SetEnvironmentVariable("MC_SPOTIFY_LOGIN_URL", authorizeUrl, EnvironmentVariableTarget.User);

        await _httpProvisioningService.StartAsync(_configuration.RedirectUri);
    }

    public async Task WaitForAuthorizationAsync()
    {
        TokenData? tokenData = await _tokenStoreService.GetTokenAsync();

        if (CheckAuthorizationIsValid(tokenData))
            return;

        _logger.LogInformation("Awaiting for authorzation finish for service '{service}'.", Name);

        while (!CheckAuthorizationIsValid(tokenData))
        {
            tokenData = await _tokenStoreService.GetTokenAsync();
            await Task.Delay(100);
        }
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
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier }
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
            { "client_id", _configuration.ClientId }
        }.Concat(additionalParams));

        var response = await _httpClient.PostAsync(_configuration.TokenEndpoint, requestContent);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve token. Status code: {statusCode}", response.StatusCode);
            return null;
        }

        return JsonConvert.DeserializeObject<SpotifyTokenResponse>(content);
    }

    private async Task SaveTokenAsync(SpotifyTokenResponse token)
    {
        var tokenData = new TokenData
        {
            AccessToken = token.Token,
            RefreshToken = token.Refresh,
            ExpiresIn = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        };

        await _tokenStoreService.SaveTokenAsync(tokenData);
    }

    private bool CheckAuthorizationIsValid(TokenData? tokenData)
    {
        return tokenData != null && tokenData.ExpiresIn > _clock.Now && !string.IsNullOrWhiteSpace(tokenData.AccessToken);
    }

    public void Dispose()
    {
        if (_httpProvisioningService.IsRunning)
        {
            _httpProvisioningService.HttpMessageReceived -= TryAuthorizationAsync;
            _httpProvisioningService.StopAsync().Wait();
        }

        GC.SuppressFinalize(this);
    }
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