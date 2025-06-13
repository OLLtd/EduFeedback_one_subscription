using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.ServiceModels
{
    public class Client_CourseTest_MapppingModel
    {

        public int Course_Test_ID { get; set; }
        public int? Course_ID { get; set; }
        public int? Organisation_ID { get; set; }
        public string CourseTestName { get; set; }
        public string? Detail { get; set; }
        public string? AssignmentName { get; set; }
        public string? AssignmentFilePath { get; set; }
        public string? GuidanceName { get; set; }
        public string? GuidanceFilePath { get; set; }
        public string? SolutionFileName { get; set; }
        public string? SolutionFilePath { get; set; }
        public string? Comment { get; set; }
        public string? GDriveLink { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public int? TotalStudentCount { get; set; }
        public int? TotalSyncStudent { get; set; }
        public bool? IsActive { get; set; }
        public string? CourseTestName_FolderId { get; set; }
        public int? TestType_ID { get; set; }
        public int? Subject_ID { get; set; }
        public int? Year_ID { get; set; }
    }

}
