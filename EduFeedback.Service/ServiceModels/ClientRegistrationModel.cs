using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EduFeedback.Service.ServiceModels
{
    public class ClientRegistrationModel
    {
        public int OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public int Year_ID { get; set; }
        public string YearName { get; set; }
        public int Subject_ID { get; set; }
        public int Course_Test_ID { get; set; }
        public string TestName { get; set; }

        public string Week { get; set; }

        public string Comment { get; set; }
        public string gDriveLink { get; set; }
        public bool StudentNameMapping { get; set; }
        public HttpPostedFileBase TestPaperFile { get; set; }

        public HttpPostedFileBase TestPaperSolutionFile { get; set; }

        public DateTime? ExamDate { get; set; }
        public IEnumerable<SelectListItem> SchoolYearList { get; set; }

        public IEnumerable<SelectListItem> CourseNameList { get; set; }

        public IEnumerable<SelectListItem> SubjectList { get; set; }
    }
}
