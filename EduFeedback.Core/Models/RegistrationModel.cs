using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.Models
{
    public class RegistrationModel
    {
        public int User_ID { get; set; }
        public string UserName { get; set; }
        public string Email_ID { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public int Year_ID { get; set; }
        public string FirstName { get; set; }
    }
}
