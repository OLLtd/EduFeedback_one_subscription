namespace EduFeedback.Web.Models
{
    public static class GoogleDriveConstants
    {
        public static readonly string ClientId = System.Configuration.ConfigurationManager.AppSettings["GoogleDrive.ClientId"];
        public static readonly string ClientSecret = System.Configuration.ConfigurationManager.AppSettings["GoogleDrive.ClientSecret"];
        public static readonly string RedirectUri = System.Configuration.ConfigurationManager.AppSettings["GoogleDrive.RedirectUri"];
        public static readonly string ApplicationName = System.Configuration.ConfigurationManager.AppSettings["GoogleDrive.ApplicationName"];
    }
}
