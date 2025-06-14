using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Models
{
    public class CoursePurchaseModel
    {
        public string Strip_PK_Key { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string CardName { get; set; }
        public int Parent_ID { get; set; }
        public int Log_ID { get; set; }
        public string Status { get; set; }
        public string Txn_Type_Status { get; set; }
        public string PurchasePromoCode { get; set; }
        public int Course_ID { get; set; }
        public string Course_Name { get; set; }
        public decimal? Course_Fee { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? Mutli_Purchase_ID { get; set; }
        public int Year_ID { get; set; }
        public string TokenID { get; set; }
        public string Product_ID { get; set; }
        public int AssignmentPerWeek { get; set; }
    }
}
