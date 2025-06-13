using EduFeedback.Common;
using EduFeedback.Service.Models;
using EduFeedback.Service.Services;
using EduFeedback.Web.Filters;
using EduFeedback.Web.Models;
using EduFeedback.Web.Services;
using Google.Apis.Drive.v3.Data;
using Hangfire;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using WebGrease;

namespace EduFeedback.Web.Controllers
{
    [Authorize]
    public class ClientDashboardController : BaseController
    {
        // private readonly IWebHostEnvironment _environment;
        //private readonly HttpClient _httpClient;
        private GoogleDriveService _googleDriveService = new GoogleDriveService();
        private SuperAdminService _superAdminService = new SuperAdminService();
        private StudentInfoService _studentInfoService = new StudentInfoService();


        private readonly BackgroundJobService _backgroundJobService;

        public ClientDashboardController(BackgroundJobService backgroundJobService)
        {
            _backgroundJobService = backgroundJobService;
        }
        public ClientDashboardController()
        {

        }

        public ActionResult Authenticate()
        {

            //Ensure that the access_type parameter is set to offline in the authentication URL.Additionally, make sure that the user grants offline access during the authentication process.

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?scope=https://www.googleapis.com/auth/drive.readonly&access_type=offline&response_type=code&redirect_uri={GoogleDriveConstants.RedirectUri}&client_id={GoogleDriveConstants.ClientId}&prompt=consent";

            //If the access_type is already set to offline and the refresh_token is still not being returned, it could be because the user has already granted offline access to your application.In such cases, Google may not return the refresh_token again. To handle this, you can force the consent screen to be shown again by adding the prompt = consent parameter to the authentication URL:
            return Redirect(authUrl);
        }

        //public ClientDashboardController()
        //{
        //    // _environment = environment;
        //    //_httpClient = httpClient;
        //    _googleDriveService = new GoogleDriveService();
        //}
        // GET: ClientDashboard
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateSessionValue(string key, string value)
        {
            if (key == "OrgId")
            {
                var orgDetail = new RegistrationService().GetOrganisationDetail(Convert.ToInt32(value));
                if (orgDetail != null)
                {
                    HttpContext.Session["EmailCode"] = orgDetail.EmailCode ?? "";
                }
            }
            HttpContext.Session[key] = value;
            return Json(new { success = true });
        }

        [HttpGet]
        public JsonResult GetOrgEmailCode()
        {
            string emailcode = HttpContext.Session["EmailCode"].ToString();
            return Json(new { data = emailcode }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStudentNameMapping(int orgId)
        {
            var organisation = new RegistrationService().GetOrganisationDetail(orgId);// Fetch organisation details from DB using orgId
            var studentNameMapping = organisation.StudentNameMapping; // Assuming this property exists
            return Json(new { success = true, studentNameMapping = studentNameMapping }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCurrentOrganisationId()
        {
            try
            {
                int orgId = CommonHelper.GetCurrentOrganisationId();
                return Json(new { OrgId = orgId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return Json(new { OrgId = 0, Error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> UploadAssignment()
        {
            //BackgroundJob.Enqueue(() => Console.WriteLine("Hangfire is working!"));
            // Google Drive authentication code -- dont move from top

            var userName = User.Identity.Name;
            var userService = new RegistrationService();
            var userData = userService.GetUserByUserName(userName);
            var refreshToken = RefreshTokenService.GetRefreshToken(userData.User_ID, userName);

            if (string.IsNullOrEmpty(refreshToken))
            {
                return RedirectToAction("Authenticate");
            }

            var accessToken = await new GoogleDriveServiceAuthenticationController().GetAccessTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Authenticate");
            }
            //----------------------------------------------

            var courseLit_GCSE = await GetAllClientTestNameList_GCSE();
            var rootObject_GCSE = JsonConvert.DeserializeObject<Client_CourseTest_Mappping_Request>(courseLit_GCSE);

            var CourseNameList = rootObject_GCSE.data.OrderByDescending(c => c.Course_Test_ID)
        .Take(10).Select(c => new SelectListItem
        {
            Value = c.Course_Test_ID.ToString(),
            Text = c.CourseTestName.ToString()
        });

            var courseLit_L2w = await GetAllClientTestNameList_L2W();
            if (courseLit_L2w != "")
            {
                courseLit_L2w = courseLit_L2w.Replace("courseTestId", "course_Test_Id");
                var rootObject_L2W = JsonConvert.DeserializeObject<Client_CourseTest_Mappping_Request>(courseLit_L2w);
                var CourseNameList_l2w = rootObject_L2W.data.OrderByDescending(c => c.Course_Test_ID)
        .Take(10).Select(c => new SelectListItem
        {
            Value = c.Course_Test_ID.ToString(),
            Text = c.CourseTestName.ToString()
        });
                CourseNameList = CourseNameList.Concat(CourseNameList_l2w);

            }

            //var combinedCourseList = courseLit_GCSE.Concat(courseLit_L2w).ToList();

            //var rootObject = JsonConvert.DeserializeObject<Client_CourseTest_Mappping_Request>(combinedCourseList);
            //return Json(new { data = rootObject.data }, JsonRequestBehavior.AllowGet);

            //var CourseNameList = rootObject.data.Select(c => new SelectListItem
            //{
            //    Value = c.Course_Test_ID.ToString(),
            //    Text = c.CourseTestName.ToString()
            //});

            //var CourseNameList = new List<SelectListItem>
            //{
            //    new SelectListItem
            //    {
            //        Value = "",
            //        Text = "Please select school year"
            //    }
            //};


            var schoolYearList = userService.GetSchoolYearlist().Select(c => new SelectListItem
            {
                Value = c.Year_ID.ToString(),
                Text = c.YearName.ToString()
            });

            var subjectList = userService.SubjectList().Select(c => new SelectListItem
            {
                Value = c.ID.ToString(),
                Text = c.SubjectName.ToString()
            });

            var orgModel = new OrganisationModel();
            var orgId = CommonHelper.GetCurrentOrganisationId();
            if (orgId > 0)
            {
                orgModel = userService.GetOrganisationDetail(orgId);
            }

            var model = new ClientRegistrationModel
            {
                ExamDate = DateTime.Now,
                SubjectList = subjectList,
                SchoolYearList = schoolYearList,
                OrganisationName = orgModel?.OrganisationName ?? string.Empty,
                CourseNameList = CourseNameList,
                StudentNameMapping = orgModel?.StudentNameMapping ?? false
            };

            return View(model);
        }


        [HttpGet]
        public async Task<ActionResult> GetFilesFromDrive(string driveLink)
        {
            // check Gmail login authantication
            var userName = User.Identity.Name;
            var userData = new RegistrationService().GetUserByUserName(userName);
            var refreshToken = RefreshTokenService.GetRefreshToken(userData.User_ID, userName);


            if (string.IsNullOrEmpty(refreshToken))
            {
                return RedirectToAction("Authenticate");
            }
            var accessToken = await new GoogleDriveServiceAuthenticationController().GetAccessTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Authenticate");
            }
            //=====================================
            // Extract the folder ID from the drive link
            var folderId = GoogleDriveService.ExtractFolderIdFromLink(driveLink);
            // Get the list of files from Google Drive
            List<GDriveFilesModel> files = await _googleDriveService.GetFilesListAsync(accessToken, folderId);

            var orgModel = new OrganisationModel();
            var orgId = CommonHelper.GetCurrentOrganisationId();
            if (orgId > 0)
            {
                orgModel = new RegistrationService().GetOrganisationDetail(orgId);
            }

            if (orgModel != null)
            {

                if (orgModel.StudentNameMapping)
                {
                    //  fileName format   -- schoolYear_studentName
                    foreach (var file in files)
                    {
                        try
                        {
                            int year = Convert.ToInt32(file.Name.Split('_')[0].Substring(1));
                            file.YearId = year;
                        }
                        catch
                        {
                            file.YearId = 0;
                        }
                        //file.Name = 
                    }

                    _studentInfoService.GetStudentList(files);
                }
            }

            return Json(new { data = files, status = "File uploaded Failed : " }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UploadAssignmentNew()
        {

            RegistrationService _service = new RegistrationService();
            IEnumerable<SelectListItem> items = _service.GetSchoolYearlist().Select(c => new SelectListItem
            {
                Value = c.Year_ID.ToString(),
                Text = c.YearName.ToString()
            });
            IEnumerable<SelectListItem> itemsSubject = _service.SubjectList().Select(c => new SelectListItem
            {
                Value = c.ID.ToString(),
                Text = c.SubjectName.ToString()
            });

            var orgModel = new OrganisationModel();
            var _org_ID = CommonHelper.GetCurrentOrganisationId();
            if (_org_ID > 0)
            {
                orgModel = _service.GetOrganisationDetail();
            }

            var model = new ClientRegistrationModel()
            {
                ExamDate = DateTime.Now,
                SubjectList = itemsSubject,
                SchoolYearList = items,
                OrganisationName = (orgModel != null) ? orgModel.OrganisationName : string.Empty
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> BulkUploadFilesNew(HttpPostedFileBase[] files)
        {
            var model = new ClientRegistrationModel();
            if (!string.IsNullOrEmpty(Request.Form["Year_ID"]))
                model.Year_ID = Convert.ToInt32(Request.Form["Year_ID"]);

            if (!string.IsNullOrEmpty(Request.Form["Subject_ID"]))
                model.Subject_ID = Convert.ToInt32(Request.Form["Subject_ID"]);

            //if (!string.IsNullOrEmpty(Request.Form["ExamDate"]))
            //    model.ExamDate = Convert.ToDateTime(Request.Form["ExamDate"]);

            if (!string.IsNullOrEmpty(Request.Form["TestName"]))
                model.TestName = Request.Form["TestName"];

            var subjectName = GetSubjectDetailByID(model.Subject_ID);

            Guid orgGUID = GetCurrentOrgGuid();

            var uploadTasks = new List<Task>();

            using (var client = new HttpClient())
            {
                for (int i = 0; i < files.Length; i++)
                {
                    HttpPostedFileBase file = files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        // System.Threading.Thread.Sleep(20000);
                        uploadTasks.Add(UploadFileToApiAsync(client, file, orgGUID, subjectName, model));
                        // uploadTasks.Add(Task.Run(() => UploadFileToApiAsync(client, file, orgGUID, subjectName, model)));
                    }
                }
            }

            // Trigger the uploads and return immediately
            await Task.WhenAll(uploadTasks);//.ContinueWith(t => { /* Handle completion */ });
            return Json(new { data = true, status = "File uploaded Successfully : " }, JsonRequestBehavior.AllowGet);
        }

        private async Task UploadFileToApiAsync(HttpClient httpClient, HttpPostedFileBase fileObj, Guid orgGUID, string subjectName, ClientRegistrationModel model)
        {
            using (var apiData = new MultipartFormDataContent())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;

                apiData.Add(new StringContent(orgGUID.ToString()), "Org_Id");
                apiData.Add(new StringContent("Comments"), "Comments");

                apiData.Add(new StringContent(fileObj.FileName), "OriginalFileName");


                var studentName = string.Empty;
                string fileName = Path.GetFileNameWithoutExtension(fileObj.FileName);
                // Find the position of "-" or "_"
                int separatorIndex = fileName.IndexOfAny(new[] { '-', '_' });
                if (separatorIndex != -1)
                    studentName = fileName.Substring(0, separatorIndex);
                else
                    studentName = fileName;

                var MaskedTestName = string.Format("{0}Y{1}{2}", GetOrganisationPrefix(), model.Year_ID, model.TestName.ToUpper());

                var updatedFileName = string.Format("{0}_{1}-{2}_{3}_{4}",
                    studentName, MaskedTestName, subjectName, "{rollno}", DateTime.Now.ToString("ddMMyyyyhhmmss") + Path.GetExtension(fileObj.FileName));

                // Create ByteArrayContent from file bytes
                var fileContent = new StreamContent(fileObj.InputStream);
                // Add file content to the form data with a unique name
                apiData.Add(fileContent, "files", string.Format("{0}", updatedFileName));

                apiData.Add(new StringContent(Path.GetFileNameWithoutExtension(updatedFileName)), "FileName");
                // string _statusCounter = string.Format("{0}/{1}", fileCounter,files.Length);

                // return Json(new { data = true, status = _statusCounter });//, JsonRequestBehavior.AllowGet);

                var response = await httpClient.PostAsync(ClientAPI.GET_BulkUpload_Api, apiData).ConfigureAwait(false);

                // var responseRead = response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                // return "true";
            }
        }

        // calculate fibonacci series       

        [HttpPost]
        public async Task<ActionResult> GDriveBulkUploadFiles(string testPapers, string yearId, string subjectId, string examDate, string testName, HttpPostedFileBase testPaperFile, HttpPostedFileBase testPaperSolutionFile, string comment, string gDriveLink = "", int applicatonService = 1)//ClientRegistrationModel model, HttpPostedFileBase[] files)
        {


            GoogleDriveServiceAuthenticationController googleDriveServiceAuthenticationController = new GoogleDriveServiceAuthenticationController();
            //var refreshToken = Session["GoogleDrive.RefreshToken"] as string;

            RegistrationService _service = new RegistrationService();
            var userName = User.Identity.Name;
            var UserData = _service.GetUserByUserName(userName);
            var refreshToken = RefreshTokenService.GetRefreshToken(UserData.User_ID, userName);

            if (string.IsNullOrEmpty(refreshToken))
            {
                return RedirectToAction("Authenticate");
            }
            var accessToken = await googleDriveServiceAuthenticationController.GetAccessTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Authenticate");
            }


            // Save the uploaded files to a temporary location
            var tempFolderPath = Server.MapPath("~/App_Data/Temp");
            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }

            string testPaperFilePath = "";
            if (testPaperFile != null && testPaperFile.ContentLength > 0)
            {
                testPaperFilePath = Path.Combine(tempFolderPath, Path.GetFileName(testPaperFile.FileName));
                testPaperFile.SaveAs(testPaperFilePath);
            }
            string testPaperSolutionFilePath = "";
            if (testPaperSolutionFile != null && testPaperSolutionFile.ContentLength > 0)
            {
                testPaperSolutionFilePath = Path.Combine(tempFolderPath, Path.GetFileName(testPaperSolutionFile.FileName));
                testPaperSolutionFile.SaveAs(testPaperSolutionFilePath);
            }


            //Arun kumar2_NFT2024MK0192024-Physics_rollno_638498403068385488.pdf
            //Arun kumar2_NFT2024MK0192024-Physics_10_638498403068385488.pdf
            // Example of an asynchronous operation

            //_ = Task.Run(async() => async_GDriveBulkUploadFiles(testPapers, yearId, subjectId, examDate, testName, testPaperFile, testPaperSolutionFile)).ConfigureAwait(false);

            // Enqueue the long-running process using Hangfire
            // Enqueue the long-running process using Hangfire
            //BackgroundJob.Enqueue<BackgroundJobService>(service => service.LongRunningProcess());

            var orgGUID = GetCurrentOrgGuid();
            var testPapersList = JsonConvert.DeserializeObject<List<GDriveFilesModel>>(testPapers);
            //var getAssignmentstatus = AssignmentService.GetAssignmentsList(testName);
            //int 
            //if (getAssignmentstatus != null && getAssignmentstatus.Count > 0)
            //{ 

            //}
            insertAssignmentsForStatus(testPapersList, testName, orgGUID);

            HttpContext.Session["SchoolYear"] = yearId;
            string SmartFileUploadApi = ClientAPI.GET_SmartFileUpload_Api;
            string UploadTestPaperApi = ClientAPI.GET_UploadTestPaper_Api;
            //BackgroundJob.Enqueue(() => Console.WriteLine("Hangfire is working!"));
            BackgroundJob.Enqueue<BackgroundJobService>(service => service.GDriveBulkUploadFiles(accessToken, testPapers, yearId, subjectId, examDate, testName, comment, gDriveLink, orgGUID, GetOrganisationPrefix(), testPaperFilePath, testPaperSolutionFilePath, SmartFileUploadApi, UploadTestPaperApi));
            //testPaperFile, testPaperSolutionFile));

            //// Resolve BackgroundJobService and call LongRunningProcess
            //var backgroundJobService = container.GetInstance<BackgroundJobService>();
            //Task.Run(() => backgroundJobService.LongRunningProcess());


            //return Json(new { data = true, status = "File uploaded Successfully : " }, JsonRequestBehavior.AllowGet);
            // Redirect to the ViewUploadTestList action after the operation completes
            // Return JSON response with redirection URL
            return Json(new { success = true, redirectUrl = Url.Action("UploadingAssignmentStatusList") });
            //return RedirectToAction("ViewUploadTestList");
        }


        [HttpPost]
        public async Task<ActionResult> UploadReuploadFile(string assignmentAllocationId, string CourseTestId, string StudentId, string StudentName, HttpPostedFileBase file)
        {



            GoogleDriveServiceAuthenticationController googleDriveServiceAuthenticationController = new GoogleDriveServiceAuthenticationController();
            //var refreshToken = Session["GoogleDrive.RefreshToken"] as string;

            RegistrationService _service = new RegistrationService();
            var userName = User.Identity.Name;
            var UserData = _service.GetUserByUserName(userName);
            var refreshToken = RefreshTokenService.GetRefreshToken(UserData.User_ID, userName);

            if (string.IsNullOrEmpty(refreshToken))
            {
                return RedirectToAction("Authenticate");
            }
            var accessToken = await googleDriveServiceAuthenticationController.GetAccessTokenAsync(refreshToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Authenticate");
            }


            // Save the uploaded files to a temporary location
            var tempFolderPath = Server.MapPath("~/App_Data/Temp");
            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }

            var orgGUID = GetCurrentOrgGuid();


            string updateFilePath = "";
            if (file != null && file.ContentLength > 0)
            {
                updateFilePath = Path.Combine(tempFolderPath, Path.GetFileName(file.FileName));
                file.SaveAs(updateFilePath);
            }




            bool response = false;

            using (var httpClient = new HttpClient())
            using (var apiData = new MultipartFormDataContent())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;

                apiData.Add(new StringContent(orgGUID.ToString()), "Org_Id");
                apiData.Add(new StringContent(StudentId), "StudentId");
                apiData.Add(new StringContent(StudentName), "StudentName");
                apiData.Add(new StringContent(CourseTestId), "CourseTestId");


                StreamContent testPaperFileContent = null;

                try
                {
                    if (!string.IsNullOrEmpty(updateFilePath))
                    {
                        var testPaperFile = new FileInfo(updateFilePath);
                        if (testPaperFile.Exists)
                        {
                            var fileStream = testPaperFile.OpenRead();
                            testPaperFileContent = new StreamContent(fileStream);
                            apiData.Add(testPaperFileContent, "file", testPaperFile.Name);
                        }
                    }


                    var apiResponse = await httpClient.PostAsync(ClientAPI.POST_UpdateAssignemntFile_Api, apiData).ConfigureAwait(false);
                    var responseRead = await apiResponse.Content.ReadAsStringAsync();
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        response = true;
                    }
                    else
                    {
                        // Log the error response
                        //LogError($"API call failed with status code: {apiResponse.StatusCode}, response: {responseRead}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    //LogError($"Exception occurred: {ex.Message}");
                }
                finally
                {
                    testPaperFileContent?.Dispose();
                }

                // Clean up the temporary files after processing
                DeleteFileIfExists(updateFilePath);
            }
            //return Json(new { success = true, redirectUrl = Url.Action("UploadingAssignmentStatusList") });
            return RedirectToAction("ViewUploadTestList");
        }


        private void DeleteFileIfExists(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var file = new FileInfo(filePath);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
        }
        public void insertAssignmentsForStatus(List<GDriveFilesModel> gDriveFilesModels, string testName, Guid orgGuid)
        {

            foreach (var x in gDriveFilesModels)
            {
                AssignmentService.AddAssignmentList(testName, x.FileID, x.ModifiedFileName, orgGuid);
            }
            //AssignmentService.AddAssignmentList(testName, FileId, FileName);
        }


        [HttpPost]
        public async Task<ActionResult> BulkUploadFiles(ClientRegistrationModel model, HttpPostedFileBase[] files)
        {
            BulkUploadRequest uploadRequest = new BulkUploadRequest();
            //// uploadRequest.files = new List<BulkFileModel>();
            //if (!Directory.Exists(HttpContext.Server.MapPath("/Bundle/XLE/")))
            //{
            //    DirectoryInfo di = Directory.CreateDirectory(HttpContext.Server.MapPath("/Bundle/XLE/"));
            //}
            var subjectName = GetSubjectDetailByID(model.Subject_ID);

            if (files != null)
            {
                Guid orgGUID = GetCurrentOrgGuid();

                // Save the uploaded files to a temporary location
                var tempFolderPath = Server.MapPath("~/App_Data/Temp");
                List<string> filesList = new List<string>();
                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }




                string testPaperFilePath = "";
                if (model.TestPaperFile != null && model.TestPaperFile.ContentLength > 0)
                {

                    testPaperFilePath = Path.Combine(tempFolderPath, Path.GetFileName(model.TestPaperFile.FileName));
                    model.TestPaperFile.SaveAs(testPaperFilePath);
                }

                string testPaperSolutionFilePath = "";
                if (model.TestPaperSolutionFile != null && model.TestPaperSolutionFile.ContentLength > 0)
                {
                    testPaperSolutionFilePath = Path.Combine(tempFolderPath, Path.GetFileName(model.TestPaperSolutionFile.FileName));
                    model.TestPaperSolutionFile.SaveAs(testPaperSolutionFilePath);
                }

                List<GDriveFilesModel> testPapersList = new List<GDriveFilesModel>();
                foreach (var f in files)
                {
                    GDriveFilesModel testPaper = new GDriveFilesModel
                    {
                        FileID = f.FileName + "_fileId",
                        Name = f.FileName,
                        ModifiedFileName = f.FileName
                    };

                    testPapersList.Add(testPaper);

                    var filePath = Path.Combine(tempFolderPath, Path.GetFileName(f.FileName));
                    f.SaveAs(filePath);
                    filesList.Add(filePath);
                }


                //JsonConvert.DeserializeObject<List<GDriveFilesModel>>(testPapers);
                insertAssignmentsForStatus(testPapersList, model.TestName, orgGUID);

                var orgPrefix = GetOrganisationPrefix();

                try
                {
                    model.TestPaperFile = null;
                    HttpContext.Session["SchoolYear"] = model.Year_ID;
                    string SmartFileUploadApi = ClientAPI.GET_SmartFileUpload_Api;
                    string UploadTestPaperApi = ClientAPI.GET_UploadTestPaper_Api;
                    BackgroundJob.Enqueue<BackgroundJobService>(service => service.UploadFileAPIInOneGo(orgGUID, model, subjectName, filesList, testPapersList, orgPrefix, testPaperFilePath, testPaperSolutionFilePath, model.Year_ID, SmartFileUploadApi, UploadTestPaperApi));
                }
                catch (Exception ex)
                {
                }
                //await UploadFileAPIInOneGo(orgGUID, model, subjectName, filesList, testPapersList, orgPrefix);

                return Json(new { data = true, status = "File uploaded Successfully : " }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = true, status = "File uploaded Failed : " }, JsonRequestBehavior.AllowGet);
        }

        //private async Task<string> UploadFileAPIInOneGo(Guid orgGUID, ClientRegistrationModel model, HttpPostedFileBase[] filePaths, string subjectName)
        //private async Task<string> UploadFileAPIInOneGo(Guid orgGUID, ClientRegistrationModel model, string subjectName, List<string> filePathList, List<GDriveFilesModel> testPapersList, string orgPrefix)
        ////HttpPostedFileBase[] filePaths,
        //{


        //    // Process files in chunks of 10
        //    //int chunkSize = 10;
        //    //int totalFiles = testPapersList.Count();
        //    //var fileNameList = testPapersList.Select(p => p.Name).ToList();
        //    //for (int i = 0; i < totalFiles; i += chunkSize)          

        //    //    var chunkFiles = testPapersList.Skip(i).Take(chunkSize);
        //    try
        //    {
        //        int loopcount = -1;
        //        foreach (var fileObj in testPapersList)
        //        {
        //            loopcount++;
        //            using (var httpClient = new HttpClient())
        //            using (var apiData = new MultipartFormDataContent())
        //            {
        //                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
        //                httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
        //                //  apiData.Add(new StringContent(""), "comments");
        //                apiData.Add(new StringContent(orgGUID.ToString()), "Org_Id");
        //                apiData.Add(new StringContent(model.Comment), "Comment");
        //                apiData.Add(new StringContent("Not available"), "GDriveLink");
        //                apiData.Add(new StringContent(model.TestName), "TestName");
        //                apiData.Add(new StringContent(model.Year_ID.ToString()), "Year");

        //                //  apiData.Add(new StringContent(fileObj.FileName), "OriginalFileName");
        //                var studentName = string.Empty;
        //                string fileName = Path.GetFileNameWithoutExtension(fileObj.Name);
        //                // Find the position of "-" or "_"
        //                int separatorIndex = fileName.IndexOfAny(new[] { '-', '_' });
        //                if (separatorIndex != -1)
        //                    studentName = fileName.Substring(0, separatorIndex);
        //                else
        //                    studentName = fileName;

        //                var MaskedTestName = string.Format("{0}Y{1}{2}", orgPrefix, model.Year_ID, DateTime.Now.ToString("yyyyMMdd"));

        //                //Prarthana Kadloor_XLE2324Y9WT4-Physics_XL202390008_fe071eaf139764a075a94d51f159c6e4 (1)
        //                var updatedFileName = string.Format("{0}_{1}-{2}_{3}_{4}",
        //                    studentName, MaskedTestName, subjectName, "{rollno}", DateTime.Now.ToString("ddMMyyyyhhmmss") + Path.GetExtension(fileObj.Name));

        //                // var fileCompletePath = HttpContext.Server.MapPath("/Bundle/XLE/") + updatedFileName;
        //                //Save all images to path
        //                // fileObj.SaveAs(fileCompletePath);
        //                // Create ByteArrayContent from file bytes



        //                apiData.Add(new StringContent(fileName), "DefaultFileName");

        //                var dd = filePathList[loopcount].ToString();
        //                FileInfo fileInfo = new FileInfo(filePathList[loopcount].ToString());
        //                using (FileStream fileStream = fileInfo.OpenRead())
        //                {
        //                    var fileContent = new StreamContent(fileStream);
        //                    // Add file content to the form data with a unique name
        //                    apiData.Add(fileContent, "file", string.Format("{0}", updatedFileName));


        //                    apiData.Add(new StringContent(Path.GetFileNameWithoutExtension(updatedFileName)), "FileName");

        //                    //var response = await httpClient.PostAsync(ClientAPI.GET_SmartFileUpload_Api, apiData).ConfigureAwait(false);
        //                    //var responseRead = response.Content.ReadAsStringAsync();
        //                    var response = await httpClient.PostAsync(ClientAPI.GET_SmartFileUpload_Api, apiData).ConfigureAwait(false);
        //                    var responseRead = response.Content.ReadAsStringAsync();

        //                    if (response.IsSuccessStatusCode)
        //                    {
        //                        AssignmentService.UpdateAssignmentUploadStatus("Uploaded", fileObj.Name + "_fileId", orgGUID, model.TestName);
        //                    }
        //                    else
        //                    {
        //                        AssignmentService.UpdateAssignmentUploadStatus("Error", fileObj.Name + "_fileId", orgGUID, model.TestName);
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return "true";
        //}

        private async Task<string> GetExamList(string examDate = "", string service = "")
        {

            Guid org_GUID = GetCurrentOrgGuid();

            using (var httpClient = new HttpClient())
            //using (var apiData = new MultipartFormDataContent())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                // httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                //  apiData.Add(new StringContent(org_GUID.ToString()), "Org_Id");

                var requestModel = new ExamListRequest()
                {
                    Org_Id = org_GUID.ToString(),
                    examDate = examDate
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                //  var Api_Url = "http://localhost:5013/api/v3/GCSEAPIV3/BulkUpload";

                var Api_Url = ClientAPI.GET_EXAM_LIST;
                //var Api_Url = (service == "1" ? ClientAPI.GCSE_GET_EXAM_LIST : ClientAPI.L2W_GET_EXAM_LIST);

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                return responseRead.Result.ToString();
            }
        }

        private async Task<string> GetAllClientTestNameList_GCSE()
        {

            Guid org_GUID = GetCurrentOrgGuid();

            using (var httpClient = new HttpClient())
            using (var apiData = new MultipartFormDataContent())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);

                //apiData.Add(new StringContent(org_GUID.ToString()), "Org_Id");

                var requestModel = new ExamListRequest()
                {
                    Org_Id = org_GUID.ToString()
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.GCSE_GET_AALCLIENTTESTNAME_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                return responseRead.Result.ToString();
            }
        }

        private async Task<string> GetAllClientTestNameList_L2W()
        {

            Guid org_GUID = GetCurrentOrgGuid();

            using (var httpClient = new HttpClient())
            using (var apiData = new MultipartFormDataContent())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);

                //apiData.Add(new StringContent(org_GUID.ToString()), "Org_Id");

                var requestModel = new ExamListRequest()
                {
                    Org_Id = org_GUID.ToString()
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.L2W_GET_AALCLIENTTESTNAME_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                return responseRead.Result.ToString();
            }
        }

        private static Guid GetCurrentOrgGuid()
        {
            RegistrationService _service = new RegistrationService();
            Guid org_GUID = new Guid();
            var _org_ID = CommonHelper.GetCurrentOrganisationId();
            if (_org_ID > 0)
            {//"60503D87-66ED-489C-B347-6542BF073752"
                var orgData = _service.GetOrganisationDetail(_org_ID);
                org_GUID = (orgData != null) ? (Guid)orgData.Org_ID : Guid.Empty;
            }

            return org_GUID;
        }

        public string GetSubjectDetailByID(int Subject_ID)
        {
            string _subject = string.Empty;
            RegistrationService _service = new RegistrationService();
            var subject = _service.SubjectDetail(Subject_ID);
            if (subject != null)
            {
                if (subject.SubjectName.Contains("Mathematics"))
                    _subject = "Maths";
                else
                    _subject = subject.SubjectName;
            }

            return _subject;
        }

        public ActionResult GetOrganisationList()
        {
            // Fetch the list of organisations from your data source
            var organisations = _superAdminService.GetOrganisationsList();
            return Json(new { data = organisations }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckParentExist(int parentId, string studentName)
        {
            // check parent exist or not
            var parentExist = _studentInfoService.CheckStudentExist(parentId, studentName);
            return Json(new { data = parentExist }, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult CheckStudentNameExist(String studentName)
        //{
        //    // check parent exist or not
        //    var parentExist = _studentInfoService.CheckStudentExist(studentName);
        //    return Json(new { data = parentExist }, JsonRequestBehavior.AllowGet);
        //}


        public ActionResult GetOrganisationDetail()
        {
            RegistrationService _service = new RegistrationService();
            var orgModel = new OrganisationModel();
            var _org_ID = CommonHelper.GetCurrentOrganisationId();
            if (_org_ID > 0)
            {
                orgModel = _service.GetOrganisationDetail(_org_ID);
            }
            return Json(new { data = orgModel }, JsonRequestBehavior.AllowGet);
        }

        private string GetOrganisationPrefix()
        {

            string MaskedTestName = string.Empty;
            RegistrationService _service = new RegistrationService();
            var orgModel = new OrganisationModel();
            var _org_ID = CommonHelper.GetCurrentOrganisationId();
            if (_org_ID > 0)
            {
                orgModel = _service.GetOrganisationDetail(_org_ID);
                MaskedTestName = (orgModel != null) ? orgModel.OrganisationName.ToUpper() : string.Empty;
                MaskedTestName += DateTime.Now.Year;
            }
            return MaskedTestName;
        }

        public async Task<ActionResult> ViewUploadTestList(string testDate = "")
        {
            DateTime searchDate = string.IsNullOrEmpty(testDate)
                ? DateTime.Now
                : DateTime.ParseExact(testDate, "dd-MM-yyyy", null);

            Session["SchoolYear"] = 7;
            //var dateLists = await GetTestActiveDateList();
            //var rootObject = JsonConvert.DeserializeObject<ExamActiveDateListObject>(dateLists);

            //IEnumerable<SelectListItem> items = rootObject.data.Select(c => new SelectListItem
            //{

            //    Value = c.Date.ToString("dd-MM-yyyy"),
            //    Text = c.Date.ToString("dd-MM-yyyy")
            //});

            IEnumerable<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "", // Empty value for "None"
                    Text = "None"
                }
            };

            var model = new TestAssignmentFilterModel()
            {
                ExamDate = searchDate,
                ExamDateList = items
            };
            return View(model);
        }

        public async Task<ActionResult> GetExamListAPI(string examDate = "")
        {

            var list = new List<ExamDisplayListModel>();

            if (string.IsNullOrEmpty(examDate))
                return Json(new { data = list }, JsonRequestBehavior.AllowGet);

            RegistrationService _service = new RegistrationService();
            var orgModel = await GetExamList(examDate);

            if (string.IsNullOrEmpty(orgModel))
                return Json(new { data = new List<ExamDisplayListModel>() }, JsonRequestBehavior.AllowGet);

            var rootObject = JsonConvert.DeserializeObject<RootObject>(orgModel);
            return Json(new { data = rootObject.data }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> UploadingAssignmentStatusList(string TestName = "Tets1")
        {
            Guid orgId = GetCurrentOrgGuid();

            var testNameLists = AssignmentService.GetTestNameList(orgId);
            //var rootObject = JsonConvert.DeserializeObject<AssignmentUploadedListRootObject>(testNameLists);


            IEnumerable<SelectListItem> items = testNameLists.Select(c => new SelectListItem
            {

                Value = c,
                Text = c
            });

            var model = new AssignmentUploadFilterModel()
            {
                TestName = TestName,
                TestNameList = items
            };
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> GetAssignmentsListAPI(string testName = "")
        {

            List<AssignmentUploadStatusModel> assignemntList = AssignmentService.GetAssignmentsList(testName);
            return Json(new { data = assignemntList }, JsonRequestBehavior.AllowGet);

            ////var dd = await GoogleDriveService.Download("1MHECl1iIp5XaSpDPfK0XoU9htjBZ1Bs4", "TestingDownloadPDF");
            //return Json(new { data = files, status = "File uploaded Failed : " }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFeedbackAssignmentList()
        {
            RegistrationService _service = new RegistrationService();
            var orgModel = new OrganisationModel();
            var _org_ID = CommonHelper.GetCurrentOrganisationId();
            if (_org_ID > 0)
            {
                orgModel = _service.GetOrganisationDetail(_org_ID);
            }
            return Json(new { data = orgModel }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> ViewUploadedFileList(int Test_ID = 0, int SubjectId = 0)
        {
            RegistrationService _service = new RegistrationService();
            var clientMastdata = await GetClientCourseDetails(Test_ID, SubjectId);
            ViewBag.CourseTestId = Test_ID;
            ViewBag.SubjectId = SubjectId;
            var rootObject = JsonConvert.DeserializeObject<ExamHeaderRootObject>(clientMastdata);
            //ViewBag.SubjectId = SubjectId;
            //   ViewBag.CourseName = (rootObject != null) ? rootObject.courseName : string.Empty;
            ViewBag.TestName = (rootObject != null) ? rootObject.data.testName : string.Empty;
            ViewBag.SubjectName = (rootObject != null) ? rootObject.data.subjectName : string.Empty; ;

            var model = new AllocationModel();
            return View(model);
        }



        public async Task<ActionResult> GetAssignmentListByCourseTestId(int CourseTestId = 0, int SubjectId = 0)
        {
            // var dataCourseList = new List<ClientStudentAllocationModel>();
            var dataCourseList = await GetAssignmentListByTestID(CourseTestId, SubjectId);
            var rootObject = JsonConvert.DeserializeObject<ExamAssignmentListRootObject>(dataCourseList);

            return Json(new { data = rootObject.data }, JsonRequestBehavior.AllowGet);
        }
        private async Task<string> GetClientCourseDetails(int Test_ID, int SubjectId)
        {

            Guid org_GUID = GetCurrentOrgGuid();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                // httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                //  apiData.Add(new StringContent(org_GUID.ToString()), "Org_Id");

                var requestModel = new ClientExamRequest()
                {
                    org_Id = org_GUID.ToString(),
                    exam_ID = Test_ID,
                    subject_Id = SubjectId
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.GET_EXAM_Detail_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                return responseRead.Result.ToString();
            }
        }

        private async Task<string> GetAssignmentListByTestID(int CourseTestId, int SubjectId)
        {

            Guid org_GUID = GetCurrentOrgGuid();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                // httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                //  apiData.Add(new StringContent(org_GUID.ToString()), "Org_Id");

                var requestModel = new ClientExamRequest()
                {
                    org_Id = org_GUID.ToString(),
                    exam_ID = CourseTestId,
                    subject_Id = SubjectId
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.GET_AssignmentList_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                return responseRead.Result.ToString();
            }
        }

        public async Task<ActionResult> GetExamTestName(string pSearchDate = "")
        {

            Guid org_GUID = GetCurrentOrgGuid();

            if (string.IsNullOrEmpty(pSearchDate))
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                // httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                // httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                //  apiData.Add(new StringContent(org_GUID.ToString()), "Org_Id");

                var requestModel = new ClientExamNameRequest()
                {
                    org_Id = org_GUID.ToString(),
                    exam_Date = pSearchDate,
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.GET_ExamName_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<ExamNameModel>(responseRead.Result.ToString());
                // return  await responseRead.Result.ToString();
                return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
            }
            // return Json(new { data = true, status = "File uploaded Failed : " }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetCourseTestDetail(int courseTestId, string courseTestName)
        {


            var Api_Url = ClientAPI.GET_CLIENTTESTNAMEBYID_Api;
            if (courseTestId <= 0 && string.IsNullOrEmpty(courseTestName))

                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                var yeardata = (courseTestName.Split('_')[2]).ToString();
                string yearString = Regex.Match(yeardata, @"Y(\d+)").Groups[1].Value; // Extract digits after 'Y'
                int yearId = int.Parse(yearString); // Convert to int (e.g., 2, 10, 11)

                //int yearId = Convert.ToInt32(yeardata.Substring(1, 1).ToString());
                if (yearId < 7)
                {
                    //Api_Url = ClientAPI.L2W_GET_CLIENTTESTNAMEBYID_Api;

                    Client_CourseTest_MapppingModel client =
                        new Client_CourseTest_MapppingModel();

                    client.Year_ID = yearId;
                    return Json(new { data = client }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    Api_Url = ClientAPI.GCSE_GET_CLIENTTESTNAMEBYID_Api;

                }

            }



            Guid org_GUID = GetCurrentOrgGuid();

            if (courseTestId <= 0)
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);

                var requestModel = new ClientExamNameRequest()
                {
                    org_Id = org_GUID.ToString(),
                    Course_Test_ID = courseTestId
                };

                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                //var Api_Url = ClientAPI.GET_CLIENTTESTNAMEBYID_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();


                //var resultString = responseRead.Result.Replace("", "").ToString();


                var result = JsonConvert.DeserializeObject<Client_CourseTest_Mappping_Request>(responseRead.Result.ToString());
                // return  await responseRead.Result.ToString();
                return Json(new { data = result.data[0] }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> ViewUploadsByTestName(string testName = "")
        {
            DateTime searchDate = DateTime.Now;
            ViewBag.TestName = testName;
            var dateLists = await GetTestActiveDateList();
            IEnumerable<SelectListItem> items = dateLists.Select(c => new SelectListItem
            {
                Value = c.ToString(),
                Text = c.ToString()
            });
            ViewBag.TestActiveDateList = items;
            return View();
        }

        public async Task<ActionResult> GetTestActiveDateList_ajax()
        {

            var dateList = await GetTestActiveDateList();
            if (string.IsNullOrEmpty(dateList))
                return Json(new { data = new List<ExamActiveDateListObject>() }, JsonRequestBehavior.AllowGet);

            var rootObject = JsonConvert.DeserializeObject<ExamActiveDateListObject>(dateList);




            //IEnumerable<SelectListItem> items = rootObject.data.Select(c => new SelectListItem
            //{

            //    Value = c.Date.ToString("dd-MM-yyyy"),
            //    Text = c.Date.ToString("dd-MM-yyyy")
            //});
            ////DateTime examDate = DateTime.ParseExact(request.ExamDate, "dd-MM-yyyy", null);

            //var model = new TestAssignmentFilterModel()
            //{
            //    ExamDate = searchDate,
            //    ExamDateList = items
            //};

            return Json(new { data = rootObject.data }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetTestNameList(String service = "gcse")
        {
            Guid orgId = GetCurrentOrgGuid();
            //List<string> testNameLists = AssignmentService.GetTestNameList(orgId);
            if (service == "1")  // 1 means GCSE, 2 Means L2W
            {

                var courseLit_GCSE = await GetAllClientTestNameList_GCSE();
                var rootObject_GCSE = JsonConvert.DeserializeObject<Client_CourseTest_Mappping_Request>(courseLit_GCSE);

                var CourseNameList = rootObject_GCSE.data.OrderByDescending(c => c.Course_Test_ID)
            .Take(10).Select(c => new SelectListItem
            {
                Value = c.Course_Test_ID.ToString(),
                Text = c.CourseTestName.ToString()
            });

                return Json(new { data = CourseNameList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var courseLit_L2w = await GetAllClientTestNameList_L2W();
                courseLit_L2w = courseLit_L2w.Replace("courseTestId", "course_Test_Id");
                var rootObject_L2W = JsonConvert.DeserializeObject<Client_CourseTest_Mappping_Request>(courseLit_L2w);
                var CourseNameList_l2w = rootObject_L2W.data.OrderByDescending(c => c.Course_Test_ID)
        .Take(10).Select(c => new SelectListItem
        {
            Value = c.Course_Test_ID.ToString(),
            Text = c.CourseTestName.ToString()
        });
                return Json(new { data = CourseNameList_l2w }, JsonRequestBehavior.AllowGet);

            }


            //var rootObject = JsonConvert.DeserializeObject<AssignmentUploadedListRootObject>(testNameLists);
            //var dateList = await GetTestActiveDateList();
            //if (string.IsNullOrEmpty(testNameLists))
            //    return Json(new { data = new List<String>() }, JsonRequestBehavior.AllowGet);

            //var rootObject = JsonConvert.DeserializeObject<List<string>>(testNameLists);           
            //return Json(new { data = testNameLists }, JsonRequestBehavior.AllowGet);
            //return Json(new { data = rootObject.data }, JsonRequestBehavior.AllowGet);
        }


        private async Task<string> GetTestActiveDateList()
        {
            Guid org_GUID = GetCurrentOrgGuid();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                //var requestModel = new ExamListRequest()
                //{
                //    Org_Id = org_GUID.ToString(),
                //    TestName = string.Empty
                //};
                var requestModel = new
                {
                    Org_Id = org_GUID.ToString(),
                    TestName = string.Empty
                };
                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                //var Api_Url = (service == "1" ? ClientAPI.GCSE_GET_ACTIVE_TESTDATE_List_Api : ClientAPI.L2W_GET_ACTIVE_TESTDATE_List_Api);
                var Api_Url = ClientAPI.GET_ACTIVE_TESTDATE_List_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);

                var responseRead = response.Content.ReadAsStringAsync();

                return responseRead.Result.ToString();
            }
        }

        public async Task<ActionResult> DownloadSingleFile(int AssignmentAllocationId = 0, string fileType = "")
        {
            Guid org_GUID = GetCurrentOrgGuid();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                var requestModel = new
                {
                    Org_Id = org_GUID.ToString(),
                    AssignmentAllocation_ID = AssignmentAllocationId,
                    FileType = fileType
                };
                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.Download_SINGLE_FILE_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseRead = response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<DownloadFileResponse>(responseRead.Result.ToString());

                if (result != null && result.data != null && result.data.fileBlob != null)
                {
                    // Decode the Base64 string to a byte array
                    byte[] fileBytes = Convert.FromBase64String(result.data.fileBlob);

                    // Define the file name and content type (adjust as needed)
                    string fileName = result.data.fileName;
                    //int lastUnderscoreIndex = fileName.LastIndexOf('_');
                    //int secondLastUnderscoreIndex = fileName.LastIndexOf('_', lastUnderscoreIndex - 1);

                    //if (secondLastUnderscoreIndex != -1)
                    //{
                    //    fileName = fileName.Substring(0, secondLastUnderscoreIndex) + ".pdf";
                    //    // Use the extractedPart as needed
                    //    //Console.WriteLine(extractedPart);
                    //}

                    if (!fileName.Contains(".pdf"))
                    {
                        fileName = fileName + ".pdf";
                    }


                    // Change the extension based on the file type
                    // string contentType = "application/octet-stream"; // Adjust content type based on the file type
                    string contentType = "application/zip";
                    // Return the file for download
                    return File(fileBytes, contentType, fileName);
                }

                else
                    return null;
            }
        }

        public async Task<ActionResult> DownloadFeedbackMultiFiles(int TestId = 0, string name = "")
        {
            Guid org_GUID = GetCurrentOrgGuid();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                var requestModel = new
                {
                    Org_Id = org_GUID.ToString(),
                    Test_ID = TestId,
                };
                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.Download_FEEDBACK_FILES_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseRead = response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<DownloadMultipleFileResponse>(responseRead.Result.ToString());
                Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

                if (result == null)
                    return null;
                foreach (var res in result.data)
                {
                    // Decode the Base64 string to a byte array
                    byte[] fileBytes = Convert.FromBase64String(res.fileBlob);

                    // Define the file name and content type (adjust as needed)
                    string fileName = res.fileName; // Change the extension based on the file type
                                                    // string contentType = "application/octet-stream"; // Adjust content type based on the file type

                    if (!fileName.Contains(".pdf"))
                    {
                        fileName = fileName + ".pdf";
                    }
                    files.Add(fileName, fileBytes);
                }
                byte[] zipFile = CommonHelper.CreateZip(files);

                var fileZipName = name + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".zip";
                return File(zipFile, "application/zip", fileZipName);
            }

            // return null;
        }

        public async Task<ActionResult> DownloadSolutionPaper(int CourseTestId = 0, string fileType = "")
        {
            Guid org_GUID = GetCurrentOrgGuid();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                var requestModel = new
                {
                    Org_Id = org_GUID.ToString(),
                    Course_Test_Id = CourseTestId,
                    FileType = fileType
                };
                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.Download_QuestionOrSolutionPaper_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseRead = response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<DownloadFileResponse>(responseRead.Result.ToString());

                if (result != null && result.data != null)
                {
                    // Decode the Base64 string to a byte array
                    byte[] fileBytes = Convert.FromBase64String(result.data.fileBlob);

                    // Define the file name and content type (adjust as needed)
                    string fileName = result.data.fileName;

                    // Change the extension based on the file type
                    // string contentType = "application/octet-stream"; // Adjust content type based on the file type
                    string contentType = "application/zip";
                    // Return the file for download
                    return File(fileBytes, contentType, fileName);
                }

                else
                    return null;
            }
        }

        public async Task<ActionResult> DownloadQuestionSolutionPaper(int CourseTestId = 0, string fileType = "")
        {
            Guid org_GUID = GetCurrentOrgGuid();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                var requestModel = new
                {
                    Org_Id = org_GUID.ToString(),
                    Course_Test_Id = CourseTestId,
                    FileType = fileType
                };
                // Step 3: Serialize parameters to JSON
                string jsonContent = JsonConvert.SerializeObject(requestModel);

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var Api_Url = ClientAPI.Download_QuestionOrSolutionPaper_Api;

                var response = await httpClient.PostAsync(Api_Url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseRead = response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<DownloadFileResponse>(responseRead.Result.ToString());

                if (result != null && result.data != null)
                {
                    // Decode the Base64 string to a byte array
                    byte[] fileBytes = Convert.FromBase64String(result.data.fileBlob);

                    // Define the file name and content type (adjust as needed)
                    string fileName = result.data.fileName;

                    // Change the extension based on the file type
                    // string contentType = "application/octet-stream"; // Adjust content type based on the file type
                    string contentType = "application/zip";
                    // Return the file for download
                    return File(fileBytes, contentType, fileName);
                }

                else
                    return null;
            }
        }

    }

}
