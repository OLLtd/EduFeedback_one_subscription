using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EduFeedback.Service.ServiceModels
{
    public class TestAssignmentFilterModel
    {
        public DateTime ExamDate { get; set; }

        public string SearchDate { get; set; }

        public IEnumerable<SelectListItem> ExamDateList { get; set; }
    }



    public class AssignmentUploadFilterModel
    {
        public String TestName { get; set; }

        public string SearchTestName { get; set; }

        public IEnumerable<SelectListItem> TestNameList { get; set; }
    }

}
