using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Models
{
    [Serializable]
    public class ProductViewModel
    {
        public int Course_ID { get; set; }
        public string Product_ID { get; set; }
        public int CourseType_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public int AssignmentPerWeek { get; set; }
        public bool IsSubscription { get; set; }
        public decimal DiscountAmount { get; set; }
        public int DiscountPercent { get; set; }
        public string Promo_Code { get; set; }
        public bool IsAlreadyPurchased { get; set; }
        public bool IsFreeCourseAlreadyPurchased { get; set; }
        public int ExamDateId { get; set; }
        public String ExamDateTime { get; set; }

        public string Strip_PK_Key { get; set; }
    }
}
