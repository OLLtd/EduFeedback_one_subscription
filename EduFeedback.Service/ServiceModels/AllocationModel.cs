using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EduFeedback.Service.ServiceModels
{
    public  class AllocationModel
    {
        public int ID { get; set; }

        public int? Parent_ID { get; set; }

        public string ParentName { get; set; }

        public string StudentName { get; set; }

        public int Organisation_ID { get; set; }
        public IEnumerable<SelectListItem> OrganisationList { get; set; }

        public int Status_ID { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public string Email_ID { get; set; }

        public int? Course_ID { get; set; }

        public string CourseName { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime CourseEndDate { get; set; }

        public int Assignment_ID { get; set; }

        public string AssignmentName { get; set; }

       // public List<EvaluatorListModel> EvaluatorList { get; set; }



      //  public List<AssignmentTypeIDModel> AssignmentTypeIDList { get; set; }

        public bool? Is_Class_Promoted { get; set; }

        public DateTime? FeedbackDate { get; set; }

        public bool? Is_Scheduled { get; set; }

        public int? Group_ID { get; set; }

        public int? SubjectLevel_ID { get; set; }

        public int Month_ID { get; set; }
        public int? Year_ID { get; set; }
        public IEnumerable<SelectListItem> Months { get; set; }
        public IEnumerable<SelectListItem> Years { get; set; }
    }
}
