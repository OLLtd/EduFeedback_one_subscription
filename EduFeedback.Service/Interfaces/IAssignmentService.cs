using EduFeedback.Service.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Interfaces
{
    public interface IAssignmentService
    {

        public bool AddAssignmentList(string testName, string FileId, String FileName, Guid OrgId);
        public  List<AssignmentUploadStatusModel> GetAssignmentsList(string testName);
        public List<String> GetTestNameList(Guid OrgId);
        public  void UpdateAssignmentUploadStatus(String status, string fileId, Guid OrgId);
    }
}
