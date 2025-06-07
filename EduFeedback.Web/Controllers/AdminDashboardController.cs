using EduFeedback.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EduFeedback.Web.Controllers
{
    [Authorize]
    public class AdminDashboardController : Controller
    {
        // GET: AdminDashboard
       // [EduAuthorizeAttribute]
        public ActionResult Index()
        {
            return View();
        }
    }
}