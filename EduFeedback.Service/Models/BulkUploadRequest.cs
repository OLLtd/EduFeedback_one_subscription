using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Models
{
    public class BulkUploadRequest
    {
        public string Org_Id { get; set; }

        //public List<BulkFileModel> files { get; set; }
    }
    //public class BulkFileModel
    //{
    //    public string FileName { get; set; }

    //    public IFormFile pdfBlob { get; set; }
    //}
}
