using EduFeedback.Core.DatabaseContext;
using EduFeedback.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.DatabaseContext
{
    public partial class Course_Master
    {
        public static CourseModel GetCourseDetail(int Course_ID)
        {
            var ModelData = new CourseModel();
            using (var context = new EduFeedEntities())
            {
                var desc = (from s in context.Course_Master
                            where s.Course_ID == Course_ID
                            select s).FirstOrDefault();
               
                if (desc != null)
                {
                    ModelData.Course_ID = desc.Course_ID;
                    ModelData.Short_Desc = desc.Short_Desc;
                    ModelData.Course_Desc = desc.Course_Desc;
                    ModelData.Course_Name = desc.Course_Name;
                    if (desc.Course_Fee == null)
                    {
                        desc.Course_Fee = 0;
                    }
                    ModelData.Course_Fee = desc.Course_Fee;
                    ModelData.CourseTypeID = (int)desc.CourseType;
                    ModelData.Subject_ID = (int)desc.Subject_ID;
                    ModelData.Product_ID = desc.Product_ID;
                }
                return ModelData;
            }
        }

    }
}
