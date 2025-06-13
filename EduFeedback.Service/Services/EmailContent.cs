using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Net.Mail;
using EduFeedback.Service.ServiceModels;
using log4net;
using log4net.Config;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using EduFeedback.Core.DatabaseContext;
using SendGrid.Helpers.Mail;
using SendGrid;


namespace EduFeedback.Service.Services
{
    public partial class EmailContent
    {
        private static readonly ILog myLogger = LogManager.GetLogger(typeof(EmailContent));
        public static EduFeedback.Core.DatabaseContext.EmailContent GetContent(string Code)
        {
            using (var context = new EduFeedback.Core.DatabaseContext.EduFeedEntities())
            {
                return (from s in context.EmailContents
                        where s.ContentCode.ToLower() == Code.ToLower()
                        select s).SingleOrDefault();
            }
        }


        public static string SendMail(EmailModel model)
        {
            bool isFreeTrialEmail = false;
            bool isNoReplyEmail = true;
            try
            {

                var Details = RegistrationService.GetUserByUserID(model.User_ID);
                if (Details != null)
                {
                    model.Mailto = Details.Email_ID;
                    model.UserName = Details.FirstName + " " + Details.LastName;
                }

                string EvaluatorFeedback = "";
                var URL = ConfigurationManager.AppSettings["url"];
                var content = GetContent(model.ContentCode);
                model.Subject = content.Subject;


                // email body

                model.Body = GetLogoSignature() +
                    content.Body.Replace("UserName", model.UserName).Replace("ParentName", model.ParentName) + model.AddBody + content.Footer;


                //var Host = ConfigurationManager.AppSettings["VMHost"];
                //var Port = ConfigurationManager.AppSettings["VMPort"];
                //var UserId = ConfigurationManager.AppSettings["VMUserId"];
                //var FromEmailId = ConfigurationManager.AppSettings["VMFromEmailId"];
                //var UserBccId = ConfigurationManager.AppSettings["VMUserBccId"];
                //var Password = ConfigurationManager.AppSettings["VMPassword"];
                //var SSL = ConfigurationManager.AppSettings["SSL"];
                //var DisplayName = ConfigurationManager.AppSettings["VMDisplayName"];
                //var FreeTrialBccId = string.Empty;

                //var contentFlagForNewEmail = new[] { "NEWUSER", "EFPTRIAL", "FREEMOCK", "REG_NOTTAKEN_MT" };
                //if (contentFlagForNewEmail.Contains(model.TrialUserCode))
                //{
                //    isFreeTrialEmail = true;
                //}
                //string hoftUserBccId = string.Empty;
                //if (model.OrganisationID != null && model.OrganisationID.Value == 2) //Hoft
                //{
                //    var organisationModel = Organisation.GetOrganisationDetailById(model.OrganisationID.Value);
                //    hoftUserBccId = (organisationModel.EmailContact != "") ? organisationModel.EmailContact : string.Empty;
                //}


                if (isNoReplyEmail)
                {
                    Execute(model, isFreeTrialEmail).ConfigureAwait(false);
                }


                return "Sent";
            }
            catch (Exception ex)
            {
                String strLogMessage = "Email Content > SendMail. ";
                strLogMessage += " In Method (SendMail) |MailTo: " + model.Mailto + "|Subject:" + model.Subject + "|ContentCode-" + model.ContentCode;
                myLogger.Error(strLogMessage, ex);
                return "Failed";
            }
        }



        private static string GetLogoSignature()
        {
            var URL = ConfigurationManager.AppSettings["url"];
            return "<p align='center'><img width='200px' height='90px' src='" + URL + "/ftdesign/img/gcse-feedback-email-logo.png' /><br/></p><hr><br/>";
        }

        static async Task Execute(EmailModel model, bool isFreeTrialEmail = false)
        {
            var apiKey = ConfigurationManager.AppSettings["sendgrid_api_key"];
            var ReplyToMail = ConfigurationManager.AppSettings["sendgrid_ReplyToEmail"];

            var FromEmailId = ConfigurationManager.AppSettings["sendgrid_FromEmailId"];
            var DisplayName = ConfigurationManager.AppSettings["sendgrid_DisplayName"];
            var Bccs = ConfigurationManager.AppSettings["sendgrid_Bccs"];
            var testingEmailId = ConfigurationManager.AppSettings["TestingEmailId"];
            var ContentTeamBcc = ConfigurationManager.AppSettings["content_TeamBcc"];
            var ContentAdminMail = ConfigurationManager.AppSettings["sendgrid_ToEmailId"];
            var FreeTrialBccId = ConfigurationManager.AppSettings["TrialMBccId"];

            if (isFreeTrialEmail)
            {
                FromEmailId = ConfigurationManager.AppSettings["sendgrid_FromTrialEmailId"];
                FreeTrialBccId = ConfigurationManager.AppSettings["TrialMBccId"];
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(FromEmailId, DisplayName);
            EmailAddress to = null;// new EmailAddress(model.Mailto, model.UserName);
            var bccList = new List<EmailAddress>();
            var bodyContent = model.Body + model.Footer;


            // for testing id
            if (!string.IsNullOrEmpty(testingEmailId))
                to = new EmailAddress(testingEmailId);
            else
                to = new EmailAddress(model.Mailto, model.UserName);

           

            // configure bcc email list
            if (!string.IsNullOrEmpty(Bccs))
            {
                var mailList = Bccs.Split(',').ToArray();
                foreach (var bccId in mailList)
                {
                    bccList.Add(new EmailAddress(bccId));
                }
            }

            // Email attachment lists
            //List<SendGrid.Helpers.Mail.Attachment> emailAttachmentList = AttachEmailFiles(model);

            var msg = MailHelper.CreateSingleEmail(from, to, model.Subject, "", bodyContent);
            if (bccList.Count() > 0)
            {
                msg.AddBccs(bccList);
            }
            //if (emailAttachmentList.Count() > 0)
            //{
            //    msg.AddAttachments(emailAttachmentList);
            //}

            if (!string.IsNullOrEmpty(ReplyToMail))
                msg.SetReplyTo(new EmailAddress(ReplyToMail, "Reply To"));

            var response = await client.SendEmailAsync(msg);

        }

        private static List<SendGrid.Helpers.Mail.Attachment> AttachEmailFiles(EmailModel model)
        {
            var emailAttachmentList = new List<SendGrid.Helpers.Mail.Attachment>();
            try
            {
                if (model.AttachmentPDF != null)
                {
                    foreach (var file in model.AttachmentPDF)
                    {
                        if (File.Exists(file))
                        {
                            var filename = Path.GetFileName(file);
                            emailAttachmentList.Add(new SendGrid.Helpers.Mail.Attachment
                            {
                                Content = FileToBase64String(file),
                                Type = "application/pdf",
                                Filename = (filename.ToString().IndexOf(".pdf") > 0 ? filename : (filename + ".pdf")),
                                Disposition = "attachment",
                                ContentId = Guid.NewGuid().ToString()
                            });
                        }
                    }
                }

                if (model.AllAttachmentList != null && model.AllAttachmentList.Count() > 0)
                {
                    foreach (var file in model.AllAttachmentList)
                    {
                        emailAttachmentList.Add(new SendGrid.Helpers.Mail.Attachment
                        {
                            Content = FileToBase64String(file.AttachmentPDFPath),
                            Type = "application/pdf",
                            Filename = file.AttachmentAliasName.IndexOf(".pdf") > 0 ? file.AttachmentAliasName : (file.AttachmentAliasName + ".pdf"),
                            Disposition = "attachment",
                            ContentId = Guid.NewGuid().ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return emailAttachmentList;
        }

        public static string FileToBase64String(string attachmentPDFPath)
        {
            string base64String = string.Empty;
            Byte[] bytes = File.ReadAllBytes(attachmentPDFPath);
            base64String = Convert.ToBase64String(bytes);
            return base64String;
        }

        public static string ParchSendMail(EmailModel model)
        {
            try
            {
                // var Details = Registration.GetUserByUserID(model.User_ID);
                // model.Mailto = Details.Email_ID;
                //  model.UserName = Details.FirstName + " " + Details.LastName;
                var URL = ConfigurationManager.AppSettings["url"];
                var content = GetContent(model.ContentCode);
                model.Subject = content.Subject;

                model.Body = content.Body.Replace("UserName", model.UserName).Replace("ParentName", model.ParentName) + model.AddBody + content.Footer;
                var Host = ConfigurationManager.AppSettings["PVMHost"];
                var Port = ConfigurationManager.AppSettings["PVMPort"];
                var UserId = ConfigurationManager.AppSettings["PVMUserId"];
                var UserBccId = ConfigurationManager.AppSettings["PVMUserBccId"];
                var Password = ConfigurationManager.AppSettings["PVMPassword"];
                var SSL = ConfigurationManager.AppSettings["PSSL"];

                MailMessage mail = new MailMessage();
                mail.To.Add(model.Mailto);
                mail.From = new MailAddress(UserId);
                // mail.Bcc.Add(UserBccId);
                mail.Subject = model.Subject;
                mail.IsBodyHtml = true;
                mail.Body = model.Body + model.Footer;
                if (model.AttachmentPDF != null)
                {
                    foreach (var file in model.AttachmentPDF)
                    {
                        //var FileStream = new FileStream(file, FileMode.Open);
                        mail.Attachments.Add(new System.Net.Mail.Attachment(file));
                    }

                }
                if (model.AllAttachmentList != null)
                {
                    foreach (var file in model.AllAttachmentList)
                    {
                        var PDFfileStream = new FileStream(file.AttachmentPDFPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        mail.Attachments.Add(new System.Net.Mail.Attachment(PDFfileStream, file.AttachmentAliasName + ".pdf", "application/pdf"));
                    }
                }

                SmtpClient smtp = new SmtpClient();
                smtp.Host = Host;
                smtp.Port = Int32.Parse(Port);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                (UserId, Password);
                smtp.EnableSsl = bool.Parse(SSL);
                smtp.Send(mail);
                mail.Attachments.Dispose();
                return "Sent";
            }
            catch (Exception ex)
            {
                String strLogMessage = "Parch Email Content Partial > SendMail. ";
                strLogMessage += " In Method (SendMail) , MailTo: " + model.Mailto + "|Subject:" + model.Subject + "|ContentCode-" + model.ContentCode;
                myLogger.Error(strLogMessage, ex);
                return "Failed";
            }
        }


        public static string GetDateTimeLongFormat(DateTime dt)
        {
            return JsonConvert.ToString(dt, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Utc).Replace("\"\\/Date(", "").Replace(")\\/\"", "");
        }
    }
}