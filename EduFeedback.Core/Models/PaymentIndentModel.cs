using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.Models
{
    public class PaymentIndentModel
    {
        public string ClientSecret { get; set; }
        public string InvoiceId { get; set; }
        public string SubscriptionId { get; set; }
    }
}
