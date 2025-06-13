using EduFeedback.Core.DatabaseContext;
using EduFeedback.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.DatabaseContext
{
    public partial class Course_Purchase
    {
        public static CourseModel GetCourseModelByMultiPurchase_ID(long MultiPurchase_ID, int Course_ID)
        {
            using (var context = new EduFeedEntities())
            {
                return (from s in context.Course_Purchase
                        where s.Mutli_Purchase_ID == MultiPurchase_ID
                        && s.Course_ID == Course_ID
                        select new CourseModel()
                        {
                            Log_ID = s.Log_ID,
                            Course_ID = s.Course_ID,
                            Course_Name = s.Course_Master.Course_Name

                        }).FirstOrDefault();

            }
        }
       
        public static int GetPackQuantity(int LogId)
        {
            using (var context = new EduFeedEntities())
            {
                var cpl = (from s in context.Course_Purchase
                           where s.Log_ID == LogId
                           select s).FirstOrDefault();
                if (cpl == null)
                    return 0;
                else
                    return cpl.AssignmentPerWeek.Value;
            }
        }

        public static int CreateLog(CoursePurchaseModel model)
        {
            using (var context = new EduFeedEntities())
            {
                var entity = new Course_Purchase();
                try
                {
                    entity.Parent_ID = model.Parent_ID;
                    entity.Course_ID = model.Course_ID;
                    //Video feedback adons
                    //if (model.IsPromoApplied == 1)
                    //{
                    //    entity.Applied_Promo_ID = model.PurchasePromoCode;
                    //    entity.Amount = model.PurchaseDiscountAmount;
                    //}
                    //else
                    //{
                    //    entity.Amount = model.TotalAmount;
                    //}
                    entity.Amount = model.TotalAmount;

                    CourseModel course = Course_Master.GetCourseDetail(model.Course_ID);
                    if (course != null)
                    {
                        entity.CourseType = course.CourseTypeID;
                    }
                    entity.DefaultYear_ID = model.Year_ID;
                    entity.Status = "Payment Initialized";
                    entity.CreatedDate = DateTime.UtcNow;
                    entity.CreatedBy = model.Name;
                    entity.UpdatedDate = DateTime.UtcNow;
                    entity.UpdatedBy = model.Name;
                    entity.Mutli_Purchase_ID = model.Mutli_Purchase_ID;
                    
                    context.Course_Purchase.Add(entity);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    String strLogMessage = "Course Purchase Log Partial > CreateLog. ";
                    strLogMessage += " In Method (CreateLog) , with message : " + ex.Message;
                  //  myLogger.Error(strLogMessage);
                }
                return entity.Log_ID;
            }
        }

        public static void UpdateLog(CoursePurchaseModel model)
        {
            using (var context = new EduFeedEntities())
            {
                var entity = (from log in context.Course_Purchase
                              where log.Log_ID == model.Log_ID
                              select log).FirstOrDefault();
                entity.Status = model.Status;

                //if (!string.IsNullOrEmpty(model.Txn_Type_Status))
                //{
                //    entity.Status = model.Txn_Type_Status;
                //}
                entity.UpdatedDate = DateTime.UtcNow;
                entity.UpdatedBy = entity.CreatedBy;
                context.SaveChangesAsync();
            }
            //using (var context = new EduFeedEntities())
            //{
            //    var entity1 = (from log in context.Course_Purchase
            //                   where log.Log_ID == model.Log_ID
            //                   select log).FirstOrDefault();
            //    return entity1.Evaluator_ID;
            //}
        }

        public static string IsCoursePurchased(int Parent_ID, int Log_ID)
        {
            using (var context = new EduFeedEntities())
            {
                var entity = (from a in context.TransactionLogs
                              where a.Parent_ID == Parent_ID && a.Log_ID == Log_ID && a.PaymentStatus == "Completed"
                              select a).ToList();
                if (entity.Count == 1)
                {
                    return "Yes";
                }
                else
                {
                    return "No";
                }
            }
        }
    }
}
