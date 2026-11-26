using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using SinkDrive.Models;
using DriveFileData = Google.Apis.Drive.v3.Data.File;

namespace SinkDrive.Services;

public class GoogleDriveUploadService
{
    private readonly IConfiguration _config;
    
    public GoogleDriveUploadService(IConfiguration configuration) => _config = configuration;

    private DriveService CreateService(GoogleAccount account)
    {
        if (string.IsNullOrEmpty(account.RefreshToken))
            throw new InvalidOperationException("Conta sem refresh token. Reconecte a conta Google.");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _config["Google:ClientId"]!,
                ClientSecret = _config["Google:ClientSecret"]!
            },
            Scopes = new[] { DriveService.Scope.Drive }
        });

        var credential = new UserCredential(flow, account.GoogleId, new TokenResponse
        {
            RefreshToken = account.RefreshToken
        });

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "SinkDrive"
        });
    }

    public async Task<DriveFileData> UploadAsync(
        GoogleAccount account, Stream content, string fileName,
        string contentType, string? folderId = null, CancellationToken ct = default)
    {
        using var service = CreateService(account);

        var metadata = new DriveFileData { Name = fileName };
        
        if(!string.IsNullOrWhiteSpace(folderId))
            metadata.Parents = new List<string> { folderId };
        
        var request = service.Files.Create(metadata, content, contentType);
        request.Fields = "id, name, webViewLink";

        var result = await request.UploadAsync(ct);

        if (result.Status != UploadStatus.Completed)
            throw result.Exception ?? new InvalidOperationException("Falha no upload");

        return request.ResponseBody;
    }

    public record FolderInfo(string Id, string Name);

    public async Task<List<FolderInfo>> ListFolderAsync(GoogleAccount account, string? parentId, CancellationToken ct = default)
    {
        using var service = CreateService(account);

        var parent = string.IsNullOrWhiteSpace(parentId) ? "root" : parentId.Replace("'", "\\'");

        var request = service.Files.List();
        request.Q = $"mimeType = 'application/vnd.google-apps.folder' and '{parent}' in parents and trashed = false";
        request.Fields = "files(id, name)";
        request.OrderBy = "name";
        request.PageSize = 200;

        var result = await request.ExecuteAsync(ct);
        return result.Files.Select(f => new FolderInfo(f.Id, f.Name)).ToList();
    }
}