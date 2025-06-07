using EduFeedback.Service.Models;
using EduFeedback.Web.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

public class GoogleDriveService
{

    private static readonly string applicationName = GoogleDriveConstants.ApplicationName;

    public async Task<List<GDriveFilesModel>> GetFilesListAsync(string accessToken, string DriveFolderID)
    {
        try
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
                ApplicationName = applicationName
            });

            // Define parameters of request.
            FilesResource.ListRequest request = service.Files.List();

            request.PageSize = 100;
            request.Q = $"'{DriveFolderID}' in parents";
            request.Fields = "nextPageToken, files(id, name,size,webViewLink, mimeType)";

            IList<Google.Apis.Drive.v3.Data.File> files = (await request.ExecuteAsync()).Files;

            if (files != null && files.Count > 0)
            {
                Console.WriteLine("Files:" + files.Count);
                List<GDriveFilesModel> gDriveFilesModels = ExtractFilesLists(files);
                return gDriveFilesModels;
            }
            else
            {
                Console.WriteLine("No files found.");
            }
        }
        catch
        {
            throw;
        }


        return new List<GDriveFilesModel>();
    }

    //public List<GDriveFilesModel> GetFilesListAsync(string DriveFolderID)
    //{
    //    UserCredential credential;

    //    //var CPath = $"{FileRootPath}credentials.json";
    //    var CPath = $"{FileRootPath}credentials_web.json";

    //    try
    //    {
    //        if (!File.Exists(CPath))
    //        {
    //            Console.WriteLine($"File not found: {CPath}");
    //            throw new FileNotFoundException($"File not found: {CPath}");
    //        }
    //        else
    //        {
    //            string credPath = "";
    //            using (var stream = new FileStream(CPath, FileMode.Open, FileAccess.Read))
    //            {

    //                try
    //                {
    //                    credPath = $"{FileRootPath}token.json";
    //                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
    //                    GoogleClientSecrets.Load(stream).Secrets,
    //                    Scopes,
    //                    "user",
    //                    CancellationToken.None,
    //                    new FileDataStore(credPath, true)).Result;
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine(ex.Message);
    //                    throw;// new FileNotFoundException($" - 1 File found: {credPath} == {CPath} ");
    //                }
    //            }
    //        }


    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.Message);
    //        throw;

    //    }
    //    try
    //    {
    //        var service = new DriveService(new BaseClientService.Initializer()
    //        {
    //            HttpClientInitializer = credential,
    //            ApplicationName = ApplicationName,
    //        });


    //        // Define parameters of request.
    //        FilesResource.ListRequest request = service.Files.List();

    //        request.PageSize = 100;
    //        request.Q = $"'{DriveFolderID}' in parents";
    //        request.Fields = "nextPageToken, files(id, name,size,webViewLink, mimeType)";

    //        IList<Google.Apis.Drive.v3.Data.File> files = request.Execute().Files;

    //        if (files != null && files.Count > 0)
    //        {
    //            Console.WriteLine("Files:" + files.Count);
    //            List<GDriveFilesModel> gDriveFilesModels = ExtractFilesLists(files);
    //            return gDriveFilesModels;
    //        }
    //        else
    //        {
    //            Console.WriteLine("No files found.");
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }


    //    return new List<GDriveFilesModel>();
    //}


    public static string ExtractFolderIdFromLink(string link)
    {
        // Implement logic to extract folder ID from the provided link
        // Example: https://drive.google.com/drive/folders/{folderId}
        var uri = new Uri(link);
        var segments = uri.Segments;
        return segments[segments.Length - 1];
    }


    public static List<GDriveFilesModel> ExtractFilesLists(IList<Google.Apis.Drive.v3.Data.File> files)
    {
        List<GDriveFilesModel> GDriveFilesModel = new List<GDriveFilesModel>();

        foreach (var dataRow in files)
        {
            GDriveFilesModel gDriveFilesModel = new GDriveFilesModel();
            // Check if the item is a folder
            if (dataRow.MimeType == "application/vnd.google-apps.folder")
            {
                gDriveFilesModel.Name += " (Folder)";
            }
            else if (dataRow.MimeType == "application/pdf")
            {
                gDriveFilesModel.FileType = " (File)";
                gDriveFilesModel.FileID = dataRow.Id;
                gDriveFilesModel.Name = dataRow.Name;
                // Convert size to MB
                if (dataRow.Size.HasValue)
                {
                    double sizeInMB = dataRow.Size.Value / (1024.0 * 1024.0);
                    if (sizeInMB > 10)
                    {
                        //gDriveFilesModel.Size = $"{sizeInMB:F2} MB <span style='color:red;'>(File size is >45 MB. It may not upload.)</span>";
                        gDriveFilesModel.Size = $"{sizeInMB:F2} MB <span style='color:red;'>(File size should be <10MB only. This file will not upload.)</span>";
                    }
                    else
                    {
                        gDriveFilesModel.Size = $"{sizeInMB:F2} MB";
                    }
                }
                else
                {
                    gDriveFilesModel.Size = "Unknown";
                }
                GDriveFilesModel.Add(gDriveFilesModel);
            }

        }
        GDriveFilesModel = GDriveFilesModel.OrderBy(x => x.Name).ToList();
        return GDriveFilesModel;
    }

    // Download a file asynchronously
    public static async Task<FileStreamResult> Download(string accessToken, string fileId, string fileName)
    {
        try
        {

            var services = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
                ApplicationName = applicationName,
            });



            var request = services.Files.Get(fileId);
            var stream = new MemoryStream();

            // Download file content
            await request.DownloadAsync(stream);

            stream.Position = 0; // Reset the stream position
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }
        return null;

    }

}
