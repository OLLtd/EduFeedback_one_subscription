using EduFeedback.Core.DatabaseContext;
using EduFeedback.Service.Interfaces;
using EduFeedback.Service.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace EduFeedback.Service.Services
{
    public class StudentInfoService : IStudentInfoService
    {
        public void GetStudentList(List<GDriveFilesModel> sList)
        {
            List<GDriveFilesModel> NetTcpStyleUriParser = new List<GDriveFilesModel>();
            //if (sList == null)
            //{
            //    return null;
            //}
            //int schoolYear = sList[0].YearId;

            using (var context = new EduFeedEntities())
            {
                var studentList = (from s in context.StudentInfo
                                   where s.IsActive == true
                                   //&& s.SchoolYear == schoolYear
                                   select s).ToList();

                foreach (var item in sList)
                {
                    try
                    {
                        var parent = studentList.Where(p => p.StudentName.ToLower().Equals(item.Name.Split('_')[1].ToString().ToLower().Replace(".pdf", "")) && p.SchoolYear == item.YearId).FirstOrDefault();

                        item.ParentId = 0;
                        if (parent != null)
                        {
                            item.ParentId = (int)parent.ParentID;
                            item.UserName = parent.UserName;
                        }
                        else
                        {
                            item.ParentId = 0;
                            item.UserName = "";
                        }
                    }
                    catch
                    {
                        item.ParentId = 0;
                        item.UserName = "";
                    }

                    //                    NetTcpStyleUriParser.Add(item);
                }

                //return NetTcpStyleUriParser;
            }
        }


        public bool CheckStudentExist(int parentId, string studentName)
        {
            using (var context = new EduFeedEntities())
            {
                return context.StudentInfo
                              .Any(s => s.ParentID == parentId && s.StudentName.Trim().ToLower() == studentName.Trim().ToLower());
            }
        }

        //public bool CheckStudentExist(string studentName)
        //{
        //    using (var context = new EduFeedEntities())
        //    {
        //        return context.StudentInfo
        //                      .Any(s => s.StudentName.Trim().ToLower() == studentName.Trim().ToLower());
        //    }
        //}
    }

}

