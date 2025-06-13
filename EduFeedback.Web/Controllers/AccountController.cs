using EduFeedback.Common;
using EduFeedback.Core.DatabaseContext;
using EduFeedback.Service.ServiceModels;
using EduFeedback.Service.Services;
using EduFeedback.Web.Filters;
using EduFeedback.Web.Models;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using EmailContent = EduFeedback.Service.Services.EmailContent;

namespace EduFeedback.Web.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //AdminRegisteration().ConfigureAwait(false);
            //var fileZipName =  DateTime.Now.ToString("ddMMyyyyhhmmss") + ".zip";

            //var FileName = "Arun kumar2_NFT2024MK0192024-Physics_rollno_638498403068385488.pdf";
            //var ValidDelimitersList = FileName.Split('_');

            //string sourceFile = @"C:\Users\arun2\OneDrive\Desktop\Upload Files\Light file\test.pdf";
            //string destinationFile = @"C:\Users\arun2\OneDrive\Desktop\Upload Files\Light file\";

            ////for (int i = 0; i < 23; i++)
            //{
            //    var fileName = "TEST K" + i.ToString() + ".pdf";
            //    System.IO.File.Copy(sourceFile, destinationFile + fileName, true);
            //}

            ViewBag.ReturnUrl = returnUrl;

            LoginViewModel model = new LoginViewModel();
            return View(model);
        }


        public async Task<bool> AdminRegisteration()
        {
            RegistrationService reg = new RegistrationService();

            var model = new RegistrationModel()
            {
                FirstName = "Client Admin",
                LastName = "CAdmin",
                Email_ID = "CAdmin@gmail.com",
                UserName = "CAdmin@gmail.com",
                Password = "Changeme@123",
                PhoneNo = "0",
                Year_ID = 2,
                Type = "CleintAdmin",
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

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            RegistrationService _service = new RegistrationService();
            FeatureService _feature = new FeatureService();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ModelState.IsValid && WebSecurity.Login(model.Email, model.Password, persistCookie: model.RememberMe))
            {

                // FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);

                Session["Time"] = model.time;
                Session["UserName"] = model.Email;
                //var user = await UserManager.FindAsync(model.Email, model.Password);
                //if (user != null)
                //{
                //   await SignInAsync(user, model.RememberMe);
                var UserData = _service.GetUserByUserName(Session["UserName"].ToString());
                if (UserData != null)
                {
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                     && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else //if (returnUrl == null)
                    {
                        var Defaultfeatures = _feature.DefaultFeature(UserData.User_ID);
                        Session["RoleName"] = Defaultfeatures[0].RoleName;
                        CommonHelper.SaveCurrentOrganisationId(UserData.OrganisationId);
                        CommonHelper.SaveLoggedUserID(UserData.User_ID);
                        CommonHelper.SaveLoggedUserRole(Defaultfeatures[0].RoleName);
                        CommonHelper.SaveLoggedUserEmail(UserData.Email_ID);
                        CommonHelper.SaveLoggedUserFName(UserData.FirstName);

                        var orgDetail = new RegistrationService().GetOrganisationDetail(UserData.OrganisationId);
                        if (orgDetail != null)
                        {
                            HttpContext.Session["EmailCode"] = orgDetail.EmailCode ?? "";
                        }

                        return RedirectToAction(Defaultfeatures[0].DefaultAction, Defaultfeatures[0].DefaultController);
                    }
                    //else
                    //{
                    //    return RedirectToLocal(returnUrl);
                    //}
                }

                return RedirectToLocal(returnUrl);



            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }
        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    MembershipUser userMemberShipData = null;
                    var Email_ID = "";
                    int User_ID = 0;
                    bool isEmailActivated = false;
                    using (var context = new EduFeedEntities())
                    {
                        var EmailData = (from reg in context.Registrations
                                         where reg.UserName == model.UserID
                                         select reg).SingleOrDefault();

                        if (EmailData != null)
                        {
                            isEmailActivated = IsRegisteredEmailActivated(isEmailActivated, context, EmailData);

                            if (EmailData != null && isEmailActivated)
                            {
                                Email_ID = EmailData.Email_ID;
                                User_ID = EmailData.User_ID;
                                userMemberShipData = Membership.GetUser(model.UserID);
                            }
                            else
                            {
                                // email not activated msg
                                ViewBag.ErrorMessage = "Registered email not Activated";
                                ViewBag.SuccessMessage = "";

                                return View(model);
                            }
                        }

                        if (userMemberShipData != null)
                        {
                            var token = WebSecurity.GeneratePasswordResetToken(userMemberShipData.UserName);

                            var emailModel = new EmailModel();
                            emailModel.OrganisationID = (EmailData.OrgnKey == null) ? 0 : EmailData.OrgnKey;
                            emailModel.User_ID = User_ID;
                            emailModel.ContentCode = "RESET";// "PROCOACHRESET";
                            emailModel.AddBody = "<a href='"
                               + Url.Action("ResetPassword", "Account", new { Code = token }, "https")
                               + "'>Reset your Password</a>";
                            var Result = EmailContent.SendMail(emailModel);

                            if (Result == "Sent")
                            {
                                ViewBag.SuccessMessage = "Password reset link has been sent to your registered Email Id.";
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Email sending failed, please try again !";
                                ModelState.AddModelError("", "Issue sending email");
                            }
                        }
                        else
                        {
                            // Don't reveal that the user does not exist or is not confirmed
                            return View("ForgotPasswordConfirmation");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                String strLogMessage = "Account Controller > ForgotPassword View. ";
                strLogMessage += " In Method (Post: ForgotPassword) , with message : " + ex.Message;
                //   myLogger.Error(strLogMessage);
                //ExceptionLogging.Error(strLogMessage + " " + ex.StackTrace);
            }

            return View(model);

            //if (ModelState.IsValid)
            //{
            //    var user = await UserManager.FindByNameAsync(model.UserID);
            //    if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            //    {
            //        // Don't reveal that the user does not exist or is not confirmed
            //        return View("ForgotPasswordConfirmation");
            //    }

            //    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
            //    // Send an email with this link
            //    // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            //    // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
            //    // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
            //    // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            //}

            //// If we got this far, something failed, redisplay form
            //return View(model);
        
        
        }
        private static bool IsRegisteredEmailActivated(bool isEmailActivated, EduFeedEntities context, Registration EmailData)
        {
            var IsEmailActivated = (from p in context.UserProfiles
                                    join web in context.webpages_Membership on p.UserId equals web.UserId

                                    where web.IsConfirmed == true && p.UserName == EmailData.UserName
                                    select p).SingleOrDefault();

            if (IsEmailActivated != null)
                isEmailActivated = true;
            return isEmailActivated;
        }
        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string Code)
        {
            return Code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {

            // Clear ModelState errors for Email to exclude it from validation
            ModelState.Remove("Email");

            if (ModelState.IsValid)
            {
                bool resetResponse = WebSecurity.ResetPassword(model.Code, model.Password);
                if (resetResponse)
                {
                    // Not in used 
                    // UserDevice.UpdateAllDeviceOnUserReset(User.Identity.Name);

                    return RedirectToAction("Login");
                    //ViewBag.SuccessMessage = "Successfully Changed";
                    //ViewBag.ErrorMessage = "";
                }
                else
                {
                    ViewBag.ErrorMessage = "Unable to change password!";
                    ViewBag.SuccessMessage = "";
                }
            }



            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}
            //var user = await UserManager.FindByNameAsync(model.Email);
            //if (user == null)
            //{
            //    // Don't reveal that the user does not exist
            //    return RedirectToAction("ResetPasswordConfirmation", "Account");
            //}
            //var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            //if (result.Succeeded)
            //{
            //    return RedirectToAction("ResetPasswordConfirmation", "Account");
            //}
            //AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            Session.Abandon();

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);


            return RedirectToAction("Login");


            // return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}