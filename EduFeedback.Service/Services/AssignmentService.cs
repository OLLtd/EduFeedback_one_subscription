using EduFeedback.Core.DatabaseContext;
using EduFeedback.Service.ServiceModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Service.Services
{
    public class AssignmentService
    {

        public static bool AddAssignmentList(string testName, string FileId, string FileName, Guid OrgId)
        {
            try
            {
                using (var context = new EduFeedEntities())
                {

                    AssignmentUploadSatu aus = new AssignmentUploadSatu();
                    // AssignmentUploadSatu is not defined
                    aus.TestName = testName;
                    aus.FileID = FileId;
                    aus.FileName = FileName;
                    aus.OrgId = OrgId;
                    aus.CreatedDate = DateTime.Now; // Ensure this is within the valid range

                    var rec = context.AssignmentUploadSatus.Where(x => x.FileName == FileName && x.TestName == testName && x.OrgId == OrgId && x.Status.StartsWith("Uploaded")).FirstOrDefault();
                    if (rec != null)
                    {
                        aus.Status = "Already Uploaded";
                    }
                    else
                    {
                        aus.Status = "Processing";
                    }

                    //AssignmentUploadSatu aus = new AssignmentUploadSatu();
                    //// AssignmentUploadSatu is not defined
                    //aus.TestName = testName;
                    //aus.FileID = FileId;
                    //aus.FileName = FileName;
                    //aus.Status = "Processing";
                    //aus.OrgId = OrgId;
                    //aus.CreatedDate = DateTime.Now; // Ensure this is within the valid range
                    context.AssignmentUploadSatus.Add(aus);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static List<AssignmentUploadStatusModel> GetAssignmentsList(string testName)
        {
            try
            {
                using (var context = new EduFeedEntities())
                {

                    var response = (from s in context.AssignmentUploadSatus
                                    where s.TestName.ToLower() == testName.ToLower()
                                    select new AssignmentUploadStatusModel()
                                    {
                                        Id = s.Id,
                                        TestName = s.TestName,
                                        FileID = s.FileID,
                                        FileName = s.FileName,
                                        Status = s.Status,
                                        OrgId = (Guid)s.OrgId
                                    }).ToList();
                    return response;

                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<String> GetTestNameList(Guid OrgId)
        {
            try
            {
                using (var context = new EduFeedEntities())
                {
                    var oneMonthAgo = DateTime.Now.AddMonths(-1);
                    var response = (from s in context.AssignmentUploadSatus
                                    where s.OrgId == OrgId
                                    && s.CreatedDate >= oneMonthAgo
                                    orderby s.Id descending
                                    select s.TestName).ToList();

                    // Use a HashSet to remove duplicates while preserving order
                    var seen = new HashSet<string>();
                    var distinctResponse = response.Where(testName => seen.Add(testName)).ToList();

                    return distinctResponse;

                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void UpdateAssignmentUploadStatus(String status, string fileId, string modifiedFileName, Guid OrgId, string testName)
        {
            try
            {
                using (var context = new EduFeedEntities())
                {

                    var response = (from s in context.AssignmentUploadSatus
                                    where s.OrgId == OrgId
                                    && s.FileID == fileId
                                    && s.TestName == testName
                                    && s.FileName.Equals(modifiedFileName)
                                    select s).OrderByDescending(s => s.Id).FirstOrDefault();
                    if (!response.Status.StartsWith("Already Uploaded"))
                    {
                        response.Status = status;
                        context.SaveChanges();
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }


    }
}
