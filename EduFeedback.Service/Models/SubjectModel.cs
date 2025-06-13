using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Models
{
    public class SubjectModel
    {
        public int ID { get; set; }
        public string SubjectName { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
