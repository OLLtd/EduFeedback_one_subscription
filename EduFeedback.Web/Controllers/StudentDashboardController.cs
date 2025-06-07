using EduFeedback.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EduFeedback.Web.Controllers
{

    [Authorize]
    [EduAuthorizeAttribute]
   
    public class StudentDashboardController : Controller
    {
        // GET: StudentDashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}