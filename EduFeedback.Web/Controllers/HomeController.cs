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

    public class HomeController : Controller
    {
        [Authorize]
        [EduAuthorizeAttribute]
        [InitializeSimpleMembership]
        public ActionResult Index()
        {
            AdminRegisteration().ConfigureAwait(false);

            return View();
        }


        public async Task<bool> AdminRegisteration()
        {
            RegistrationService reg = new RegistrationService();

            var model = new RegistrationModel()
            {
                FirstName = "Super Admin",
                LastName = "SAdmin",
                Email_ID = "pradeep_lko22606@yahoo.com",
                UserName = "pradeep_lko22606@yahoo.com",
                Password = "Changeme@123",
                PhoneNo = "0",
                Year_ID = 2,
                Type = "SuperAdmin",
            };

            try
            {

                int User_ID = await reg.RegisterAdminUser(model);

                if (User_ID > 0)
                {
                    // Assigned Roles to new user
                    reg.AssignRole(User_ID, model.Type);
                    /* Clear Previous session */
                    //WebSecurity.Logout();
                    // Session.Abandon();

                    // Create account 
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}