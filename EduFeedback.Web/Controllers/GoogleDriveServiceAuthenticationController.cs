using EduFeedback.Service.Services;
using EduFeedback.Web.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace EduFeedback.Web.Controllers
{
    public class GoogleDriveServiceAuthenticationController : Controller
    {
        private readonly string clientId = GoogleDriveConstants.ClientId;
        private readonly string clientSecret = GoogleDriveConstants.ClientSecret;
        private readonly string redirectUri = GoogleDriveConstants.RedirectUri;
        private readonly string applicationName = GoogleDriveConstants.ApplicationName;



        // Step 1: Authenticate
        public ActionResult Authenticate()
        {

            //Ensure that the access_type parameter is set to offline in the authentication URL.Additionally, make sure that the user grants offline access during the authentication process.

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?scope=https://www.googleapis.com/auth/drive.readonly&access_type=offline&response_type=code&redirect_uri={redirectUri}&client_id={clientId}&prompt=consent";

            //If the access_type is already set to offline and the refresh_token is still not being returned, it could be because the user has already granted offline access to your application.In such cases, Google may not return the refresh_token again. To handle this, you can force the consent screen to be shown again by adding the prompt = consent parameter to the authentication URL:
            return Redirect(authUrl);
        }

        // Step 2: Callback
        public async Task<ActionResult> Callback(string code)
        {


            if (string.IsNullOrEmpty(code))
            {
                return Content("Authorization failed.");
            }
            var tokenResponse = await GetTokensAsync(code);
            if (tokenResponse.ContainsKey("refresh_token"))
            {
                // Save the refresh token securely (e.g., database or session)
                //Session["GoogleDrive.RefreshToken"] = tokenResponse["refresh_token"];

                RegistrationService _service = new RegistrationService();
                var userName = User.Identity.Name;                
                var UserData = _service.GetUserByUserName(userName);
                RefreshTokenService.StoreRefreshToken(UserData.User_ID, userName, tokenResponse["refresh_token"], DateTime.UtcNow.AddYears(1));
            }
            return RedirectToAction("UploadAssignment", "ClientDashboard");
        }


        private async Task<Dictionary<string, string>> GetTokensAsync(string authorizationCode)
        {
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "code", authorizationCode },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(parameters));
                var responseContent = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
            }
        }

        public async Task<string> GetAccessTokenAsync(string refreshToken)
        {
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "refresh_token", refreshToken },
                    { "grant_type", "refresh_token" }
                };

                var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(parameters));
                var responseContent = await response.Content.ReadAsStringAsync();

                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                tokenData.TryGetValue("access_token", out var accessToken);

                return accessToken;
            }
        }

    }
}