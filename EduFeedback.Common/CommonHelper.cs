using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EduFeedback.Common
{
    public class CommonHelper
    {
        // private static readonly ILog _logger = LogManager.GetLogger(typeof(CommonHelper));
        /// <summary>
        /// Gets the current organisation identifier.
        /// </summary>
        /// <returns></returns>
        public static Int32 GetCurrentOrganisationId()
        {
            Int32 OrgId = 0;

            try
            {
                if (HttpContext.Current.Session["OrgId"] != null && !string.IsNullOrEmpty(HttpContext.Current.Session["OrgId"].ToString()))
                    OrgId = Convert.ToInt32(HttpContext.Current.Session["OrgId"].ToString());
                return OrgId;
            }
            catch (Exception ex)
            {
                //_logger.Error("Error in GetCurrentOrganisationId:", ex);
                throw;
            }
        }

        /// <summary>
        /// Saves the current organisation identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        public static void SaveCurrentOrganisationId(Int64 Id)
        {
            try
            {
                if (Id > 0)
                    HttpContext.Current.Session["OrgId"] = Id;

            }
            catch (Exception ex)
            {
                // _logger.Error("Error in SaveCurrentOrganisationId:", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the logged user role.
        /// </summary>
        /// <returns></returns>
        public static string GetLoggedUserRole()
        {
            string role = string.Empty;

            try
            {
                if (HttpContext.Current.Session["UserRole"] != null && !string.IsNullOrEmpty(HttpContext.Current.Session["UserRole"].ToString()))
                    role = HttpContext.Current.Session["UserRole"].ToString();
                return role;
            }
            catch (Exception ex)
            {
                //  _logger.Error("Error in GetLoggedUserRole:", ex);
                throw;
            }
        }

        /// <summary>
        /// Saves the logged user role.
        /// </summary>
        /// <param name="role">The role.</param>
        public static void SaveLoggedUserRole(string role)
        {
            try
            {
                if (!string.IsNullOrEmpty(role))
                    HttpContext.Current.Session["UserRole"] = role;
            }
            catch (Exception ex)
            {
                // _logger.Error("Error in SaveLoggedUserRole:", ex);
                throw;
            }
        }

        /// <summary>
        /// Get the logged user Email.
        /// </summary>
        /// <returns></returns>
        public static string GetLoggedUserEmail()
        {
            string role = string.Empty;

            try
            {
                if (HttpContext.Current.Session["UserEmail"] != null && !string.IsNullOrEmpty(HttpContext.Current.Session["UserEmail"].ToString()))
                    role = HttpContext.Current.Session["UserEmail"].ToString();
                return role;
            }
            catch (Exception ex)
            {
                // _logger.Error("Error in GetLoggedUserEmail:", ex);
                throw;
            }
        }

        /// <summary>
        /// Saves the logged user Email.
        /// </summary>
        /// <param name="email"></param>
        public static void SaveLoggedUserEmail(string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                    HttpContext.Current.Session["UserEmail"] = email;
            }
            catch (Exception ex)
            {
                //  _logger.Error("Error in SaveLoggedUserEmail:", ex);
                throw;
            }
        }


        public static void SaveLoggedUserFName(string name)
        {
            try
            {
                if (!string.IsNullOrEmpty(name))
                    HttpContext.Current.Session["FName"] = name;
            }
            catch (Exception ex)
            {
                //  _logger.Error("Error in SaveLoggedUserName:", ex);
                throw;
            }
        }


        public static string GetLoggedUserFName()
        {
            string role = string.Empty;

            try
            {
                if (HttpContext.Current.Session["FName"] != null && !string.IsNullOrEmpty(HttpContext.Current.Session["FName"].ToString()))
                    role = HttpContext.Current.Session["FName"].ToString();
                return role;
            }
            catch (Exception ex)
            {
                //  _logger.Error("Error in GetLoggedUserFName:", ex);
                throw;
            }
        }

        /// <summary>
        /// Saves the logged user role.
        /// </summary>
        /// <param name="role">The role.</param>
        public static void SaveLoggedUserID(int userId)
        {
            try
            {
                if (userId > 0)
                    HttpContext.Current.Session["UserID"] = userId;
            }
            catch (Exception ex)
            {
                //   _logger.Error("Error in SaveLoggedUserID:", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the logged user role.
        /// </summary>
        /// <returns></returns>
        public static int GetLoggedUserID()
        {
            int UserId = 0;

            try
            {
                if (HttpContext.Current.Session["UserID"] != null && int.Parse(HttpContext.Current.Session["UserID"].ToString()) > 0)
                    UserId = int.Parse(HttpContext.Current.Session["UserID"].ToString());
                return UserId;
            }
            catch (Exception ex)
            {
                //  _logger.Error("Error in GetLoggedUserID:", ex);
                throw;
            }
        }

        public static string FirstCharToUpper(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.  
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.  
            // ... Uppercase the lowercase letters following spaces.  
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        public static void CopySourseToDestinationPath(string sourceFilePath, string destinationFilePath)
        {

            try
            {
                if (System.IO.File.Exists(sourceFilePath))
                {
                    System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                }
            }
            catch (Exception ex)
            {
                //   _logger.Error("Error in CopySourseToDestinationPath:", ex);
                throw;
            }
        }

        public static string RemoveExtention(string fileName)
        {
            var fileSplit = fileName.Split('.');
            if (fileSplit.Length > 0)
            {
                return fileSplit[0].ToString();
            }
            return fileName;
        }

        public static string RemovelastStringPartFromString(String mainString, String removingString)
        {
            return mainString.Substring(0, mainString.Length - removingString.Length);
        }

        public static byte[] CreateZip(Dictionary<string, byte[]> files)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        ZipArchiveEntry entry = archive.CreateEntry(file.Key);
                        using (Stream entryStream = entry.Open())
                        using (MemoryStream fileStream = new MemoryStream(file.Value))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                return ms.ToArray();
            }
        }


    }
}
