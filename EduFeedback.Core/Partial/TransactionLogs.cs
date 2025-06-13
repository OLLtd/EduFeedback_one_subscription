using EduFeedback.Core.DatabaseContext;
using EduFeedback.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.DatabaseContext
{
    public partial class TransactionLogs
    {
        public static int CreateTransactionLog(TransactionModel model)
        {
            using (var context = new EduFeedEntities())
            {
                var entity = new TransactionLogs();
                try
                {
                    entity.Log_ID = model.Log_ID;
                    entity.Parent_ID = model.Parent_ID;
                    entity.Course_ID = model.Course_ID;
                    entity.Name = model.FirstName;
                    entity.PaymentStatus = model.PaymentStatus;
                    //entity.PaymentType = model.PaymentType;
                   // entity.PayerStatus = model.PayerStatus;
                    entity.PayerEmail = model.PayerEmail;
                   // entity.ReceiverId = model.ReceiverId;
                    entity.TxnType = model.TxnType;
                    entity.TxnId = model.TxnId;

                    decimal mcGrossValue = 0.0m;
                    if (!string.IsNullOrEmpty(model.Mc_gross))
                    {
                        decimal.TryParse(model.Mc_gross, out mcGrossValue);
                    }
                    entity.mc_gross = mcGrossValue;

                    entity.CreatedDate = model.CreatedDate ?? DateTime.UtcNow;
                    entity.CreatedBy = model.FullName;

                    entity.CardName = model.CardName;
                    entity.Installments = (model.Installments != null) ? model.Installments : 0;
                    entity.InvoiceId = model.InvoiceId;
                    entity.InvoiceUrl = model.InvoiceUrl;
                    entity.SubscriptionId = model.SubscriptionId;
                    entity.Quantity = (model.Quantity != null) ? model.Quantity : 0;
                    entity.CustomerID = model.CustomerID;
                    entity.TokenID = model.TokenID;
                    entity.TxnId = model.TxnId;
                    context.TransactionLogs.Add(entity);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    String strLogMessage = "Transaction Log Partial > CreateTransactionLog. ";
                    strLogMessage += " In Method (CreateTransactionLog) , with message : " + ex.Message + "Inner Exception :" + ex.GetBaseException().ToString();
                    //  myLogger.Error(strLogMessage);
                }
                return entity.ID;
            }
        }
    }
}
