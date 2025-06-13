using EduFeedback.Core.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.DatabaseContext
{
    public partial class Multi_Purchase_Order
    {
        public static int SaveMultiPurchaseOrder(int pParentID, int pTotalCourse, decimal pTotalPrice)
        {
            int orderId = 0;
            using (var context = new EduFeedEntities())
            {
                var addOrder = new Multi_Purchase_Order()
                {
                    Parent_ID = pParentID,
                    TotalCourseCount = pTotalCourse,
                    CreatedDate = DateTime.Now.ToLocalTime(),
                    TotalPrice = pTotalPrice
                };

                context.Multi_Purchase_Order.Add(addOrder);
                context.SaveChanges();
                orderId = addOrder.Mutli_Purchase_ID;
            }
            return orderId;
        }
    }
}
