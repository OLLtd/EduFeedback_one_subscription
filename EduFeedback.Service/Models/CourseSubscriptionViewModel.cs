using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EduFeedback.Service.Models
{
    public class CourseSubscriptionViewModel
    {
        public int Year { get; set; }
        public int Subject { get; set; }
        public int AssignmentsPerWeek { get; set; }
        public string PromoCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Captcha { get; set; }
        
        public IEnumerable<SelectListItem> SchoolYearList { get; set; }

        public IEnumerable<SelectListItem> SubjectList { get; set; }
    }

   
}
