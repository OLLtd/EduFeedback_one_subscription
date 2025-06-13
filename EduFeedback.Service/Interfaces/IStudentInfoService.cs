using EduFeedback.Service.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Interfaces
{
    public interface IStudentInfoService
    {
        public void GetStudentList(List<GDriveFilesModel> sList);
    }
}
