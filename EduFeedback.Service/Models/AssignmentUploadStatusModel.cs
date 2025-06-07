using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Models
{
    public class AssignmentUploadStatusModel
    {
        public int Id { get; set; }
        public string TestName { get; set; }
        public string FileID { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public Guid OrgId { get; set; }

    }
    public class AssignmentUploadRootObject
    {
        public List<AssignmentUploadStatusModel> data { get; set; }
    }
    public class AssignmentUploadedListRootObject
    {
        public List<String> data { get; set; }
    }

}
