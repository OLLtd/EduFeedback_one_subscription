using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EduFeedback.Service.ServiceModels
{
    public class GDriveFilesModel
    {
        public String FileID { get; set; }

        public String Name { get; set; }
        public String UserName { get; set; }

        public int ParentId { get; set; }

        public int YearId { get; set; }

        public String FileType { get; set; }

        public string Size { get; set; }

        public String ModifiedFileName { get; set; }

    }
}
