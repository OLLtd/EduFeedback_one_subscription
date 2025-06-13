using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.Models
{
    public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            LogIds = new List<int>();
        }
        public decimal TotalPrice { get; set; }
        public int Parent_ID { get; set; }
        public string Parent_Name { get; set; }
        public int MultiPurchase_ID { get; set; }
        public int TotalProductInCart { get; set; }
        public string TokenID { get; set; }
        public List<int> LogIds { get; set; }
        public string Strip_PK_Key { get; set; }
        public string Email { get; set; }
        public string CardName { get; set; }

        public string PurchasePromoCode { get; set; }

        public string ClientSecret { get; set; }
        public string PaymentMethodId { get; set; }
        public string CustomerId { get; set; }
        public string SubscriptionId { get; set; }
        public string IntentId { get; set; }
        public string InvoiceId { get; set; }
    }
}
