using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.ServiceModels
{
    public class ExamDisplayListModel
    {
        public int exam_ID { get; set; }
        public int subject_ID { get; set; }
        public string examName { get; set; }
        public DateTime? createdDate { get; set; }
        public string subject { get; set; }
        public string year { get; set; }
        public string totalAssignment { get; set; }
        public string totalFeedbackStatus { get; set; }
        public string questionPaperName { get; set; }
        public string solutionFileName { get; set; }
    }

    public class RootObject
    {
        public List<ExamDisplayListModel> data { get; set; }
    }
}
