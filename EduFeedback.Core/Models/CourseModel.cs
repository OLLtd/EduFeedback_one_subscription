using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EduFeedback.Models
{
    public class CourseModel
    {
        public int? Course_ID { get; set; }

        public int? Log_ID { get; set; }

        public int? AssignmentPerWeek { get; set; }

        public string Captcha { get; set; }
        public string Course_Name { get; set; }
        public decimal? Course_Fee { get; set; }
        public string Short_Desc { get; set; }
        public string Course_Desc { get; set; }
        public decimal? TotalAmount { get; set; }

        public List<NumberModel> NumberOfAssignments { get; set; }
        public int CourseTypeID { get; set; }
        public int Subject_ID { get; set; }
        public int Year_ID { get; set; }
        public string Product_ID { get; set; }
        public int Parent_ID { get; set; }
        public string PromoCode { get; set; }

        public IEnumerable<SelectListItem> SchoolYearList { get; set; }

        public IEnumerable<SelectListItem> SubjectList { get; set; }
    }
    public class NumberModel
    {
        public int Value { get; set; }

        public string Name { get; set; }
    }
}
