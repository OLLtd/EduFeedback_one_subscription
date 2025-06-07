using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Models
{
    public class ExamListRequest
    {
        public string Org_Id { get; set; }

        public string examDate { get; set; }
    }

    public class ClientExamRequest
    {
        public string org_Id { get; set; }
        public int exam_ID { get; set; }
        public int subject_Id { get; set; }
    }

    public class ClientExamNameRequest
    {
        public string org_Id { get; set; }
        public int? Course_Test_ID { get; set; }
        public string exam_Date { get; set; }
    }

    public class ExamNameModel
    {
        public object data { get; set; }
    }

    public class ExamHeaderDetailModel
    {
        public string testName { get; set; }
        public string subjectName { get; set; }
    }

    public class ExamHeaderRootObject
    {
        public ExamHeaderDetailModel data { get; set; }
    }

    public class ExamAssignmentListRootObject
    {
        public List<AssignmentAllocationModel> data { get; set; }
    }

    public class Client_CourseTest_Mappping_Request
    {
        public List<Client_CourseTest_MapppingModel> data { get; set; }
    }

    public class Client_CourseTest_Mappping_Request2
    {
        public Client_CourseTest_MapppingModel data { get; set; }
    }
    public class AssignmentAllocationModel
    {

        public string parent_ID { get; set; }
        public string studentName { get; set; }
        public int course_Test_ID { get; set; }
        public string studentRollNo { get; set; }
        public string fileName { get; set; }
        public string filePath { get; set; }
        public int assignmentAllocation_ID { get; set; }
        public DateTime? assignmentSubmitedDate { get; set; }
        public DateTime? evaluatorFeedbackDate { get; set; }
        public string feedbackUploadedPath { get; set; }
        public string feedbackFileName { get; set; }
        public string feedbackStatus { get; set; }
        public string? EvaluatorName { get; set; }
        //public string year { get; set; }
    }

    public class ExamCourseDetailModel
    {
        public int course_Test_ID { get; set; }
        public int course_ID { get; set; }
        public int subject_ID { get; set; }
        public string courseName { get; set; }
        public string testName { get; set; }
        public DateTime? syncDate { get; set; }
        public int totalStudentSync { get; set; }
        public int totalSyncFailed { get; set; }
        public int organisation_ID { get; set; }
        public string assignmentName { get; set; }
        public string assignmentFilePath { get; set; }
        public string guidanceName { get; set; }
        public string guidanceFilePath { get; set; }
        public string year { get; set; }
        public string subjectName { get; set; }
    }

    public class ExamActiveDateListObject
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<DateTime> data { get; set; }
    }

    //public class ExamActiveDate
    //{
    //    [JsonProperty("date")]
    //    [JsonConverter(typeof(CustomDateTimeConverter))]
    //    public DateTime Date { get; set; }
    //}


    //public class CustomDateTimeConverter : JsonConverter
    //{
    //    private readonly string[] _formats = { "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy" };

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType == typeof(DateTime);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.String)
    //        {
    //            string dateString = (string)reader.Value;
    //            if (DateTime.TryParseExact(dateString, _formats, null, System.Globalization.DateTimeStyles.None, out DateTime date))
    //            {
    //                return date;
    //            }
    //        }
    //        throw new JsonSerializationException($"Unable to parse date: {reader.Value}");
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        DateTime date = (DateTime)value;
    //        writer.WriteValue(date.ToString("dd-MM-yyyy HH:mm:ss"));
    //    }
    //}
    public class DownloadMultipleFileResponse
    {
        public List<DownloadFileModel> data { get; set; }
    }
    public class DownloadFileResponse
    {
        public DownloadFileModel data { get; set; }
    }
    public class DownloadFileModel
    {
        public string fileName { get; set; }
        public string fileBlob { get; set; }
    }
}
