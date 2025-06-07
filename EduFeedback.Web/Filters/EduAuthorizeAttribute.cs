using EduFeedback.Service.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EduFeedback.Web.Filters
{
    public class EduAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            FeatureService _feature = new FeatureService();
            RegistrationService _service = new RegistrationService();
            if (HttpContext.Current.Session["UserName"] == null)
            {
                RouteValueDictionary rd = new RouteValueDictionary();
                rd.Add("Controller", "Account");
                rd.Add("Action", "Login");
                filterContext.Result = new RedirectToRouteResult("Default", rd);
            }

            if (filterContext.HttpContext.Request.IsAuthenticated && HttpContext.Current.Session["UserName"] != null)
            {
                var Allfeatures = _feature.FeatureList(_service.GetUserByUserName(HttpContext.Current.Session["UserName"].ToString()).User_ID);
                var url = filterContext.HttpContext.Request.Url.ToString();
                var request = new HttpRequest(null, url, "");
                var response = new HttpResponse(new StringWriter());
                var httpContext = new HttpContext(request, response);
                var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
                var values = routeData.Values;
                var controllerName = values["controller"];
                var actionName = values["action"];
                var feature = (from f in Allfeatures
                               where f.ControllerName.ToLower() == controllerName.ToString().ToLower()
                               && f.ActionName.ToLower() == actionName.ToString().ToLower()
                               select f).SingleOrDefault();
                if (feature == null)
                {
                    var Defaultfeatures = _feature.DefaultFeature(_service.GetUserByUserName(HttpContext.Current.Session["UserName"].ToString()).User_ID);
                    RouteValueDictionary rd = new RouteValueDictionary();
                    rd.Add("Controller", Defaultfeatures[0].DefaultController);
                    rd.Add("Action", Defaultfeatures[0].DefaultAction);
                    filterContext.Result = new RedirectToRouteResult("Default", rd);
                }
            }





            //if (filterContext.HttpContext.Request.IsAuthenticated && HttpContext.Current.Session["UserName"] != null)
            //{
            //    var features = MSTR_Employee.GetRoleID(MSTR_Employee.GetEmployeeByUserName(HttpContext.Current.Session["UserName"].ToString()).EMP_ID);
            //    filterContext.Controller.ViewBag.SideNav = features;
            //    var url = filterContext.HttpContext.Request.Url.ToString();
            //    var request = new HttpRequest(null, url, "");
            //    var response = new HttpResponse(new StringWriter());
            //    var httpContext = new HttpContext(request, response);
            //    var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            //    var values = routeData.Values;
            //    var controllerName = values["controller"];
            //    var actionName = values["action"];
            //    var feature = (from f in features
            //                   where f.ControllerName.ToLower() == controllerName.ToString().ToLower()
            //                   && f.ActionName.ToLower() == actionName.ToString().ToLower()
            //                   select f).SingleOrDefault();
            //    if (feature == null)
            //    {
            //        RouteValueDictionary rd = new RouteValueDictionary();
            //        rd.Add("controller", "Error");
            //        rd.Add("action", "Index");
            //        filterContext.Result = new RedirectToRouteResult("Default", rd);
            //    }

            //}
            base.OnAuthorization(filterContext);


        }
    }
}