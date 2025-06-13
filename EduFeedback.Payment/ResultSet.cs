using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Payment
{
    public class ResultSet
    {
        public int IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
