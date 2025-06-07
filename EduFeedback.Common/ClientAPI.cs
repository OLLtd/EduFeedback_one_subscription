using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace EduFeedback.Common
{
    public static class ClientAPI
    {
        public static string GCSE_URL = ConfigurationManager.AppSettings["GCSE_API_URL"];
        public static string L2W_URL = ConfigurationManager.AppSettings["L2W_API_URL"];

        public const string Organisation_Key = "60503D87-66ED-489C-B347-6542BF073752";
        public const string XApiKey = "pgH7QzFHJx4w46fI~5Uzi4RvtTwlEXp";


        //private string GetBaseUrl(int schoolYear) => schoolYear < 7 ? feedbackSettings.Value.L2WUrl : feedbackSettings.Value.GcseUrl;

        private static string BaseUrl => GetSchoolYear() < 7 ? L2W_URL : GCSE_URL;

        public static string GET_EXAM_LIST => BaseUrl + "/GetExamList";
        public static string GET_BulkUpload_Api => BaseUrl + "/BulkUpload";
        public static string GET_SmartFileUpload_Api => BaseUrl + "/SmartFileUpload";
        public static string GET_UploadTestPaper_Api => BaseUrl + "/UploadTestPaper";
        public static string POST_UpdateAssignemntFile_Api => BaseUrl + "/UpdateAssignemntFile";
        public static string GET_EXAM_Detail_Api => BaseUrl + "/GetClientCourseDetails";
        public static string GET_AssignmentList_Api => BaseUrl + "/GetClientAssignmentListByTestId";
        public static string GET_ExamName_Api => BaseUrl + "/GetClientTestNameByDate";
        public static string GET_ACTIVE_TESTDATE_List_Api => BaseUrl + "/GetTestActiveDateList";
        public static string Download_SINGLE_FILE_Api => BaseUrl + "/DownloadSingleFile";
        public static string Download_FEEDBACK_FILES_Api => BaseUrl + "/DownloadFeedbackFiles";
        public static string Download_QuestionOrSolutionPaper_Api => BaseUrl + "/DownloadQuestionOrSolutionPaper";
        public static string GET_AALCLIENTTESTNAME_Api => BaseUrl + "/GetAllClientTestName";
        public static string GET_CLIENTTESTNAMEBYID_Api => BaseUrl + "/GetClientTestNameByID";

        public static int GetSchoolYear()
        {
            int schoolYear = 7;
            if (HttpContext.Current.Session["SchoolYear"] != null)
            {
                if (int.TryParse(HttpContext.Current.Session["SchoolYear"].ToString(), out int year))
                {
                    schoolYear = year;
                }
            }

            return schoolYear;
        }



        public static string GCSE_GET_AALCLIENTTESTNAME_Api => GCSE_URL + "/GetAllClientTestName";
        public static string GCSE_GET_CLIENTTESTNAMEBYID_Api => GCSE_URL + "/GetClientTestNameByID";

        public static string L2W_GET_AALCLIENTTESTNAME_Api => L2W_URL + "/GetAllClientTestName";
        public static string L2W_GET_CLIENTTESTNAMEBYID_Api => L2W_URL + "/GetClientTestNameByID";
    }
}
