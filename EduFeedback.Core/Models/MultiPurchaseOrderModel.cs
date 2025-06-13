using EduFeedback.Core.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.Models
{
    public class MultiPurchaseOrderModel
    {
        public int Mutli_Purchase_ID { get; set; }
        public Nullable<int> Parent_ID { get; set; }
        public Nullable<int> TotalCourseCount { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }

        public virtual Registration Registration { get; set; }
    }
}
