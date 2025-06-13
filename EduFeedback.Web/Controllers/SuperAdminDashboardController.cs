using EduFeedback.Service.Models;
using EduFeedback.Service.Services;
using EduFeedback.Web.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EduFeedback.Web.Controllers
{
    [Authorize]
    [EduAuthorizeAttribute]
    [InitializeSimpleMembership]
    public class SuperAdminDashboardController : Controller
    {
        public readonly SuperAdminService _superAdminService;

        public SuperAdminDashboardController()
        {
            _superAdminService = new SuperAdminService();
        }

        // GET: SuperAdminDashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateOrganisation()
        {
            var model = new OrganisationModel
            {
                OrganisationType = OrganisationModel.OrganisationTypeEnum.Tution
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateOrganisation(OrganisationModel organisationModel)
        {
            if (ModelState.IsValid)
            {
                var organisationExists = _superAdminService.CheckOrganisationExist(organisationModel.OrganisationName);
                if (!organisationExists)
                {
                    bool orgCreated = _superAdminService.CreateOrganisation(organisationModel);
                    if (orgCreated)
                    {
                        // Optionally, add a success message or redirect to another action
                        TempData["SuccessMessage"] = "Organisation created successfully.";
                        return RedirectToAction("ViewOrganisations");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create organisation.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Organisation already exists.");
                }
            }
            return View(organisationModel);
        }

        public ActionResult ViewOrganisations(String orgName = "")
        {
            //List<OrganisationModel> organisations = _superAdminService.GetOrganisationsList();


            //IEnumerable<SelectListItem> items = organisations.Select(c => new SelectListItem
            //{

            //    Value = c.Org_ID.ToString(),
            //    Text = c.OrganisationName.ToString()
            //});            

            //var model = new OrganisationFilterModel()
            //{
            //    Organisation = orgName,
            //    OrganisationList = items
            //};
            //return View(model);

            return View();
        }

        public async Task<ActionResult> GetOrganisationListAPI()
        {
            List<OrganisationModel> organisations = _superAdminService.GetOrganisationsList();

            //var list = new List<ExamDisplayListModel>();            
            //if (string.IsNullOrEmpty(examDate))
            //    return Json(new { data = list }, JsonRequestBehavior.AllowGet);

            //RegistrationService _service = new RegistrationService();
            //var orgModel = await GetExamList(examDate);

            //if (string.IsNullOrEmpty(orgModel))
            //    return Json(new { data = new List<ExamDisplayListModel>() }, JsonRequestBehavior.AllowGet);

            //var rootObject = JsonConvert.DeserializeObject<RootObject>(orgModel);
            return Json(new { data = organisations }, JsonRequestBehavior.AllowGet);
        }
    }
}