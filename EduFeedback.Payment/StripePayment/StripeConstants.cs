using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Payment.StripePayment
{
    public class StripeConstants
    {
        public static string Security_Key { get; set; } = ConfigurationManager.AppSettings["stripe_securitykey"].ToString();
        public static string Security_PublishKey { get; set; } = ConfigurationManager.AppSettings["stripe_publishkey"].ToString();

    }
}
