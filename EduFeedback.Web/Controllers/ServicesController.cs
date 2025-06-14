using EduFeedback.Core.DatabaseContext;
using EduFeedback.Models;
using EduFeedback.Payment.StripePayment;
using EduFeedback.Service.Models;
using EduFeedback.Service.Services;
using EduFeedback.Web.Filters;
using EduFeedback.Web.Helper.Cart;
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

            //var model = new CourseSubscriptionViewModel
            //{

            //    SubjectList = subjectList,
            //    SchoolYearList = schoolYearList,

            //};
            CourseModel model = Course_Master.GetCourseDetail(1);
            model.SubjectList = subjectList;
            model.SchoolYearList = schoolYearList;
            model.Parent_ID = 11;

            return View(model);
        }

        //[HttpPost]
        //public ActionResult OneSubscription(CourseSubscriptionViewModel model)
        //{

        //    return View(model);

        //}

        [HttpPost]
        public ActionResult OneSubscription(CourseModel model, string Name, string Email)
        {
            CoursePurchaseModel coursePayment = new CoursePurchaseModel();
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Email))
                return RedirectToAction("OneSubscription");
            try
            {
                CartHelper.AddProductToCart((int)model.Course_ID, (int)model.AssignmentPerWeek, model.Year_ID);


                return RedirectToAction("Checkout", "Cart", new { Name = Name, Email = Email });


            }
            catch (Exception ex)
            {
                String strLogMessage = "Home Management Controller > ServicesDetails ";
                strLogMessage += " In Method (Post : ServicesDetails) , with message : " + ex.Message;
                // myLogger.Error(strLogMessage + " " + ex.StackTrace);

            }

            coursePayment.Strip_PK_Key = StripeConstants.Security_PublishKey.ToString();
            return View("Stripes", coursePayment);
        }

        [AllowAnonymous]
        public ActionResult Successful()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Failure()
        {
            return View();
        }
    }
}