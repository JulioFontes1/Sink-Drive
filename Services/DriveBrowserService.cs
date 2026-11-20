using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using SinkDrive.Models;

namespace SinkDrive.Services;

public record DriveItemVm(
     string Id,
     string Name,
     string MimeType,
     bool IsFolder,
     long? Size,
     DateTime? ModifiedTime,
     string? WebViewLink,
     int AccountId,
     string AccountEmail
);

public record BreadcrumbVm(
     string Id,
     string Name
);

public class DriveBrowserService
{
     private const string FolderMime = "application/vnd.google-apps.folder";
     private const int MaxItemsPerAccount = 500;

     private readonly IConfiguration _config;

     public DriveBrowserService(IConfiguration config)
     {
          _config = config;
     }
     
     private Google.Apis.Drive.v3.DriveService CreateClient(GoogleAccount account)
     {
          var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
          {
               ClientSecrets = new ClientSecrets
               {
                    ClientId = _config["Google:ClientId"]!,
                    ClientSecret = _config["Google:ClientSecret"]!
               },
               Scopes = new[] { Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly },
          });

          var token = new TokenResponse { RefreshToken = account.RefreshToken };
          var credential = new UserCredential(flow, account.GoogleId, token);

          return new Google.Apis.Drive.v3.DriveService(new BaseClientService.Initializer
          {
               HttpClientInitializer = credential,
               ApplicationName = "SinkDrive"
          });
     }

     public async Task<List<DriveItemVm>> ListItemsAsync(GoogleAccount account, string? folderId)
     {
          var client = CreateClient(account);
          var parent = string.IsNullOrWhiteSpace(folderId) ? "root" : folderId;
          var items = new List<DriveItemVm>();
          string? pageToken = null;

          do
          {
               var request = client.Files.List();
               request.Q = $"'{parent}' in parents and trashed = false";
               request.Fields = "nextPageToken, files(id, name,mimeType, size, modifiedTime, webViewLink)";
               request.PageSize = 100;
               request.PageToken = pageToken;
               
               var result = await request.ExecuteAsync();

               foreach (var f in result.Files)
               {
                    items.Add(new DriveItemVm(
                         f.Id,
                         f.Name,
                         f.MimeType,
                         f.MimeType == FolderMime,
                         f.Size,
                         f.ModifiedTimeDateTimeOffset?.LocalDateTime,
                         f.WebViewLink,
                         account.Id,
                         account.Email));
               }
               pageToken = result.NextPageToken;
          } while (pageToken != null && items.Count < MaxItemsPerAccount);

          return items;
     }

     public async Task<List<BreadcrumbVm>> GetBreadcrumbAsync(GoogleAccount account, string folderId)
     {
          var client = CreateClient(account);
          var trail = new List<BreadcrumbVm>();
          var current = folderId;

          for (int i = 0; i < 12 && current != null; i++)
          {
               var req = client.Files.Get(current);
               req.Fields = "id, name, parents";
               var files =  await req.ExecuteAsync();
               
               trail.Insert(0, new BreadcrumbVm(files.Id, files.Name));
               current = files.Parents?.FirstOrDefault();
          }
          return trail;
     }
}    

