using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.Models
{
    public class EmailModel
    {
        public string Mailto { get; set; }
        public string Mailfrom { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string MailBcc { get; set; }
        public string AddBody { get; set; }
        public string Footer { get; set; }
        public string ContentCode { get; set; }
        public string TrialUserCode { get; set; }
        public int? User_ID { get; set; }
        public string UserName { get; set; }
        public string ParentName { get; set; }
        public string EvaluatorFeedback { get; set; }
        public string[] AttachmentPDF { get; set; }
        public List<AttachmentPDFModel> AllAttachmentList { get; set; }
        public string CourseName { get; set; }
        public string CourseSubject { get; set; }
        public string Amount { get; set; }
        public int? OrganisationID { get; set; }
        public string YearName { get; set; }
        public int? SubjectId { get; set; }

    }

    public class AttachmentPDFModel
    {
        public int AttachmentPDFID { get; set; }
        public string AttachmentPDFPath { get; set; }
        public string AttachmentAliasName { get; set; }
    }
}
