using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace SinkDrive.Services;

public record GoogleIdentity(string GoogleId, string Email, string? RefreshToken);

public class GoogleOAuthService
{
    private static readonly string[] Scopes =
    [
        "openid",
        "email",
        DriveService.Scope.DriveReadonly
    ];

    private readonly string _clientId;
    private readonly string _clientSecret;

    public GoogleOAuthService(IConfiguration configuration)
    {
        _clientId = configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId não foi configurado.");
        _clientSecret = configuration["Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google:ClientSecret não foi configurado.");
    }

    public string BuildAuthorizationUrl(string redirectUri, string state)
    {
        var request = new GoogleAuthorizationCodeRequestUrl(
            new Uri("https://accounts.google.com/o/oauth2/v2/auth"))
        {
            ClientId = _clientId,
            RedirectUri = redirectUri,
            Scope = string.Join(" ", Scopes),
            State = state,
            AccessType = "offline",
            Prompt = "consent"
        };

        return request.Build().AbsoluteUri;
    }

    public async Task<GoogleIdentity> ExchangeCodeAsync(string code, string redirectUri)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            },
            Scopes = Scopes,
            DataStore = new NullDataStore()
        });

        var token = await flow.ExchangeCodeForTokenAsync(
            "me", code, redirectUri, CancellationToken.None);

        var payload = await GoogleJsonWebSignature.ValidateAsync(
            token.IdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_clientId]
            });

        return new GoogleIdentity(payload.Subject, payload.Email, token.RefreshToken);
    }
}