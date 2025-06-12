using EduFeedback.Service.Models;
using EduFeedback.Service.Services;
using EduFeedback.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace EduFeedback.Web.Controllers
{

    public class ServicesController : Controller
    {

        public ActionResult OneSubscription()
        {
            var userService = new RegistrationService();
            var schoolYearList = userService.GetSchoolYearlist().Select(c => new SelectListItem
            {
                Value = c.Year_ID.ToString(),
                Text = c.YearName.ToString()
            });

            var subjectList = userService.SubjectList().Select(c => new SelectListItem
            {
                Value = c.ID.ToString(),
                Text = c.SubjectName.ToString()
            });

            var model = new CourseSubscriptionViewModel
            {
                
                SubjectList = subjectList,
                SchoolYearList = schoolYearList,
               
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult OneSubscription(CourseSubscriptionViewModel model)
        {

            return View(model);

        }
    }
}