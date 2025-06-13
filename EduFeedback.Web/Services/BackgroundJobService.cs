using EduFeedback.Common;
using EduFeedback.Service.ServiceModels;
using EduFeedback.Service.Services;
using EduFeedback.Web.Controllers;
using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace EduFeedback.Web.Services
{
    public class BackgroundJobService
    {

        public async Task LongRunningProcess()
        {
            // Simulate a long-running process
            await Task.Delay(10000); // Sleep for 10 seconds
            //await Task.Delay(10000);
            //await Task.Delay(10000);
            //await Task.Delay(10000);
            //await Task.Delay(10000);
            //await Task.Delay(10000);
            //await Task.Delay(10000);

            // Perform the actual long-running task here
            // For example, processing data, generating reports, etc.
        }


        [AutomaticRetry(Attempts = 3)]
        public async Task GDriveBulkUploadFiles(String accessToken, string testPapers, string yearId, string subjectId, string examDate, string testName, string comment, string gDriveLink, Guid orgGUID, String orgPrefix, string testPaperFilePath, string testPaperSolutionFilePath = "", string SmartFileUploadApi = "", string UploadTestPaperApi = "")
        // HttpPostedFileBase testPaperFile, HttpPostedFileBase testPaperSolutionFile)
        {
            var testPapersList = JsonConvert.DeserializeObject<List<GDriveFilesModel>>(testPapers);

            BulkUploadRequest uploadRequest = new BulkUploadRequest();
            ClientRegistrationModel model = new ClientRegistrationModel();
            model.Subject_ID = int.Parse(subjectId);
            model.Year_ID = int.Parse(yearId);
            model.TestName = testName;

            if (DateTime.TryParse(examDate, out DateTime parsedDate))
            {
                model.ExamDate = parsedDate;
            }
            else
            {
                model.ExamDate = null; // or handle the error as needed
            }
            var subjectName = GetSubjectDetailByID(model.Subject_ID);
            int status = 0;
            if (testPapersList != null)
            {

                status = await GDriveUploadFileAPIInOneGo(accessToken, orgGUID, model, testPapersList, subjectName, gDriveLink, comment, orgPrefix, yearId, SmartFileUploadApi).ConfigureAwait(false);

            }
            if (status == 2)
            {
                if (testPaperFilePath != "")
                {
                    await SaveQuestionPaper(orgGUID, subjectName, testName, testPaperFilePath, testPaperSolutionFilePath, yearId, UploadTestPaperApi);
                }
            }
        }

        public async Task<int> GDriveUploadFileAPIInOneGo(String accessToken, Guid orgGUID, ClientRegistrationModel model, List<GDriveFilesModel> testPapersList, string subjectName, string gDriveLink, string comment, String orgPrefix, string year, string api)// HttpPostedFileBase testPaperFile, HttpPostedFileBase testPaperSolutionFile)
        {
            // Process files in chunks of 10
            int chunkSize = 10;
            int status = 0;  // incase status= 1 then need not to upload QuestionPaper and solutionpaper
            int totalFiles = testPapersList.Count();
            var fileNameList = testPapersList.Select(p => p.ModifiedFileName).ToList();

            var getAssignmentstatus = AssignmentService.GetAssignmentsList(model.TestName);

            //insertAssignmentsForStatus(testPapersList, model.TestName, orgGUID);

            // orgGUID = SaveQuestionPaper(orgGUID, testPaperFilePath, testPaperSolutionFilePath);

            for (int i = 0; i < totalFiles; i += chunkSize)
            //for (int i = 0; i < totalFiles; i++)
            {
                //await Task.Run(() =>
                //{

                //});

                //var orgPrefix = GetOrganisationPrefix();
                var chunkFiles = testPapersList.Skip(i).Take(chunkSize);
                foreach (var fileObj in chunkFiles)
                {
                    if (ExtractDecimalValue(fileObj.Size) > 10)
                    {
                        AssignmentService.UpdateAssignmentUploadStatus("Upload Failed : File Size Limit Exceeded {" + ExtractDecimalValue(fileObj.Size) + "MB}", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);
                        continue;
                    }

                    using (var httpClient = new HttpClient())
                    using (var apiData = new MultipartFormDataContent())
                    {
                        httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                        httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                        //  apiData.Add(new StringContent(""), "comments");
                        apiData.Add(new StringContent(orgGUID.ToString()), "Org_Id");
                        apiData.Add(new StringContent(comment), "Comment");
                        apiData.Add(new StringContent(gDriveLink), "GDriveLink");
                        apiData.Add(new StringContent(model.TestName), "TestName");
                        apiData.Add(new StringContent(year), "Year");
                        apiData.Add(new StringContent(subjectName), "SubjectName");
                        apiData.Add(new StringContent(fileObj.UserName ?? ""), "UserName");



                        apiData.Add(new StringContent(fileObj.ParentId.ToString()), "ParentId");



                        //  apiData.Add(new StringContent(fileObj.FileName), "OriginalFileName");
                        var studentName = string.Empty;
                        studentName = Path.GetFileNameWithoutExtension(fileObj.ModifiedFileName);
                        apiData.Add(new StringContent(studentName), "StudentName");
                        //// Find the position of "-" or "_"
                        //int separatorIndex = fileName.IndexOfAny(new[] { '-', '_' });
                        //if (separatorIndex != -1)
                        //    studentName = fileName.Substring(0, separatorIndex);
                        //else
                        //studentName = fileName;

                        var MaskedTestName = string.Format("{0}Y{1}{2}", orgPrefix.ToString(), model.Year_ID, DateTime.Now.ToString("yyyyMMdd"));

                        //Prarthana Kadloor_XLE2324Y9WT4-Physics_XL202390008_fe071eaf139764a075a94d51f159c6e4 (1)
                        var updatedFileName = string.Format("{0}_{1}-{2}_{3}_{4}",
                            studentName, MaskedTestName, subjectName, "{rollno}", DateTime.Now.ToString("ddMMyyyyhhmmss") + Path.GetExtension(fileObj.ModifiedFileName));

                        // var updatedTestPaperFileName = testPaperFile.FileName;// model.TestName.ToString() + "_TestPaper" + Path.GetExtension(assignmentFile.FileName);

                        //var updatedAssignmentSolutionFileName = testPaperSolutionFile.FileName;// model.TestName.ToString() + "_AssignmentSolution" + Path.GetExtension(assignmentSolutionFile.FileName);

                        // var fileCompletePath = HttpContext.Server.MapPath("/Bundle/XLE/") + updatedFileName;
                        //Save all images to path
                        // fileObj.SaveAs(fileCompletePath);
                        //Create ByteArrayContent from file bytes


                        var fileStreamResult = await GoogleDriveService.Download(accessToken, fileObj.FileID, fileObj.ModifiedFileName);

                        // Extract the stream from the FileStreamResult
                        var fileStream = fileStreamResult.FileStream;

                        // Create StreamContent from the file stream                        
                        var fileContent = new StreamContent(fileStream);

                        apiData.Add(new StringContent(fileObj.Name), "DefaultFileName");
                        //apiData.Add(new StringContent(comment), "Comment");

                        //Add file content to the form data with a unique name
                        apiData.Add(fileContent, "file", string.Format("{0}", updatedFileName));

                        apiData.Add(new StringContent(Path.GetFileNameWithoutExtension(updatedFileName)), "FileName");

                        var dd = getAssignmentstatus.Where(x => x.FileID.Equals(fileObj.FileID) && x.Status.ToLower().Equals("uploaded")).ToList();


                        //var rec = context.AssignmentUploadSatus.Where(x => x.FileName == FileName && x.TestName == testName && x.OrgId == OrgId && x.Status == "Uploaded").FirstOrDefault();


                        if (dd != null && dd.Count > 0)
                        {
                            // file already uploaded for the student
                            //AssignmentService.UpdateAssignmentUploadStatus("Already Uploaded {" + ExtractDecimalValue(fileObj.Size) + " MB}", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);

                            AssignmentService.UpdateAssignmentUploadStatus("Already Uploaded {" + ExtractDecimalValue(fileObj.Size) + " MB}", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);
                        }
                        else
                        {
                            //ClientAPI.GET_SmartFileUpload_Api
                            var response = await httpClient.PostAsync(api, apiData).ConfigureAwait(false);
                            var responseRead = response.Content.ReadAsStringAsync();

                            JObject jsonResult = JObject.Parse(responseRead.Result.ToString());
                            int responsestatus = (int)jsonResult["status"];

                            if (responsestatus == 406)
                            {
                                if (status == 0)
                                {
                                    status = 1;  // if status is false
                                }
                                AssignmentService.UpdateAssignmentUploadStatus("Failed {" + ExtractDecimalValue(fileObj.Size) + " MB}", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);
                            }
                            else
                            {
                                status = 2;
                                if (response.IsSuccessStatusCode)
                                {
                                    AssignmentService.UpdateAssignmentUploadStatus("Uploaded {" + ExtractDecimalValue(fileObj.Size) + "MB}", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);

                                    //AssignmentService.UpdateAssignmentUploadStatus("Uploaded", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);
                                }
                                else
                                {
                                    AssignmentService.UpdateAssignmentUploadStatus("Failed {" + ExtractDecimalValue(fileObj.Size) + "MB}", fileObj.FileID, fileObj.ModifiedFileName, orgGUID, model.TestName);
                                }
                            }
                        }
                    }
                }


            }
            return status;
        }

        public static decimal ExtractDecimalValue(string sizeString)
        {
            if (string.IsNullOrEmpty(sizeString))
                return 0;

            // Regular expression to match a decimal number (e.g., 0.89, 73.45)
            string pattern = @"\d+\.\d+"; // Matches numbers like 0.89, 73.45
            var match = Regex.Match(sizeString, pattern);

            if (match.Success && decimal.TryParse(match.Value, out decimal result))
            {
                return result;
            }

            return 0; // Return 0 if no valid decimal is found
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

        public async Task<bool> SaveQuestionPaper(Guid orgGUID, string subjectName, string testName, string testPaperFilePath, string testPaperSolutionFilePath, string yearId, string UploadTestPaperApi)
        {
            bool response = false;

            using (var httpClient = new HttpClient())
            using (var apiData = new MultipartFormDataContent())
            {
                httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;

                apiData.Add(new StringContent(orgGUID.ToString()), "Org_Id");
                apiData.Add(new StringContent(subjectName), "SubjectName");
                apiData.Add(new StringContent(testName), "TestName");
                apiData.Add(new StringContent(yearId), "Year");

                StreamContent testPaperFileContent = null;
                StreamContent testPaperSolutionFileContent = null;

                try
                {
                    if (!string.IsNullOrEmpty(testPaperFilePath))
                    {
                        var testPaperFile = new FileInfo(testPaperFilePath);
                        if (testPaperFile.Exists)
                        {
                            var fileStream = testPaperFile.OpenRead();
                            testPaperFileContent = new StreamContent(fileStream);
                            apiData.Add(testPaperFileContent, "QuestionPaper", testPaperFile.Name);
                        }
                    }

                    if (!string.IsNullOrEmpty(testPaperSolutionFilePath))
                    {
                        var testPaperSolutionFile = new FileInfo(testPaperSolutionFilePath);
                        if (testPaperSolutionFile.Exists)
                        {
                            var fileStream = testPaperSolutionFile.OpenRead();
                            testPaperSolutionFileContent = new StreamContent(fileStream);
                            apiData.Add(testPaperSolutionFileContent, "SolutionPaper", testPaperSolutionFile.Name);
                        }
                    }
                    else
                    {
                        // Add an empty ByteArrayContent for the SolutionPaper field
                        apiData.Add(new ByteArrayContent(new byte[0]), "SolutionPaper");
                    }

                    //ClientAPI.GET_UploadTestPaper_Api
                    var apiResponse = await httpClient.PostAsync(UploadTestPaperApi, apiData).ConfigureAwait(false);
                    var responseRead = await apiResponse.Content.ReadAsStringAsync();
                    if (apiResponse.IsSuccessStatusCode)
                    {
                        response = true;
                    }
                    else
                    {
                        // Log the error response
                        LogError($"API call failed with status code: {apiResponse.StatusCode}, response: {responseRead}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    LogError($"Exception occurred: {ex.Message}");
                }
                finally
                {
                    testPaperFileContent?.Dispose();
                    testPaperSolutionFileContent?.Dispose();
                }

                // Clean up the temporary files after processing
                DeleteFileIfExists(testPaperFilePath);
                DeleteFileIfExists(testPaperSolutionFilePath);
            }

            return response;
        }
        private void LogError(string message)
        {
            Console.WriteLine(message);
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

        [AutomaticRetry(Attempts = 3)]
        public async Task UploadFileAPIInOneGo(Guid orgGUID, ClientRegistrationModel model, string subjectName, List<string> filePathList, List<GDriveFilesModel> testPapersList, string orgPrefix, string testPaperFilePath, string testPaperSolutionFilePath, int year_Id, string SmartFileUploadApi, string UploadTestPaperApi)
        //HttpPostedFileBase[] filePaths,
        {


            // Process files in chunks of 10
            //int chunkSize = 10;
            //int totalFiles = testPapersList.Count();
            //var fileNameList = testPapersList.Select(p => p.Name).ToList();
            //for (int i = 0; i < totalFiles; i += chunkSize)          

            //    var chunkFiles = testPapersList.Skip(i).Take(chunkSize);
            try
            {
                int loopcount = -1;
                foreach (var fileObj in testPapersList)
                {
                    loopcount++;
                    using (var httpClient = new HttpClient())
                    using (var apiData = new MultipartFormDataContent())
                    {
                        httpClient.DefaultRequestHeaders.Add("XApiKey", ClientAPI.XApiKey);
                        httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
                        //  apiData.Add(new StringContent(""), "comments");
                        apiData.Add(new StringContent(orgGUID.ToString()), "Org_Id");
                        apiData.Add(new StringContent(model.Comment ?? string.Empty), "Comment");
                        apiData.Add(new StringContent(""), "GDriveLink");
                        apiData.Add(new StringContent(model.TestName), "TestName");
                        apiData.Add(new StringContent(model.Year_ID.ToString()), "Year");
                        apiData.Add(new StringContent(subjectName), "SubjectName");
                        //  apiData.Add(new StringContent(fileObj.FileName), "OriginalFileName");
                        var studentName = string.Empty;
                        studentName = Path.GetFileNameWithoutExtension(fileObj.Name);
                        apiData.Add(new StringContent(studentName), "StudentName");
                        //string fileName = Path.GetFileNameWithoutExtension(fileObj.Name);
                        //// Find the position of "-" or "_"
                        //int separatorIndex = fileName.IndexOfAny(new[] { '-', '_' });
                        //if (separatorIndex != -1)
                        //    studentName = fileName.Substring(0, separatorIndex);
                        //else
                        //    studentName = fileName;

                        var MaskedTestName = string.Format("{0}Y{1}{2}", orgPrefix, model.Year_ID, DateTime.Now.ToString("yyyyMMdd"));

                        //Prarthana Kadloor_XLE2324Y9WT4-Physics_XL202390008_fe071eaf139764a075a94d51f159c6e4 (1)
                        var updatedFileName = string.Format("{0}_{1}-{2}_{3}_{4}",
                            studentName, MaskedTestName, subjectName, "{rollno}", DateTime.Now.ToString("ddMMyyyyhhmmss") + Path.GetExtension(fileObj.Name));

                        // var fileCompletePath = HttpContext.Server.MapPath("/Bundle/XLE/") + updatedFileName;
                        //Save all images to path
                        // fileObj.SaveAs(fileCompletePath);
                        // Create ByteArrayContent from file bytes


                        apiData.Add(new StringContent(fileObj.Name), "DefaultFileName");

                        var dd = filePathList[loopcount].ToString();
                        FileInfo fileInfo = new FileInfo(filePathList[loopcount].ToString());
                        using (FileStream fileStream = fileInfo.OpenRead())
                        {
                            var fileContent = new StreamContent(fileStream);
                            // Add file content to the form data with a unique name
                            apiData.Add(fileContent, "file", string.Format("{0}", updatedFileName));


                            apiData.Add(new StringContent(Path.GetFileNameWithoutExtension(updatedFileName)), "FileName");

                            //var response = await httpClient.PostAsync(ClientAPI.GET_SmartFileUpload_Api, apiData).ConfigureAwait(false);
                            //var responseRead = response.Content.ReadAsStringAsync();
                            //ClientAPI.GET_SmartFileUpload_Api
                            var response = await httpClient.PostAsync(SmartFileUploadApi, apiData).ConfigureAwait(false);
                            var responseRead = response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                AssignmentService.UpdateAssignmentUploadStatus("Uploaded", fileObj.Name + "_fileId", fileObj.Name, orgGUID, model.TestName);
                            }
                            else
                            {
                                AssignmentService.UpdateAssignmentUploadStatus("Failed", fileObj.Name + "_fileId", fileObj.Name, orgGUID, model.TestName);
                            }
                        }
                    }
                }

                if (testPaperFilePath != "" || testPaperSolutionFilePath != "")
                {
                    await SaveQuestionPaper(orgGUID, subjectName, model.TestName, testPaperFilePath, testPaperSolutionFilePath, year_Id.ToString(), UploadTestPaperApi);
                }
            }
            catch (Exception ex)
            {

            }
            //return "true";
        }

    }
}