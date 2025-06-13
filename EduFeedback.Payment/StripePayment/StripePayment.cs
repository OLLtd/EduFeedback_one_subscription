using EduFeedback.Core.DatabaseContext;
using EduFeedback.Core.Models;
using EduFeedback.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Payment.StripePayment
{
    public class StripePayment
    {
        public async Task<ResultSet> MultiCharge(CheckoutViewModel model, List<ProductViewModel> listPayment)
        {
            ResultSet resultSet = new ResultSet();
            try
            {

                string Customer_ID = model.CustomerId;  //Customer.Customer_ID;

                int CourseTypeID = 0;

                foreach (var product in listPayment)
                {
                    decimal TotalAmount = 0;
                    if (product.DiscountAmount > 0)
                        TotalAmount = (decimal)product.DiscountAmount;
                    else
                        TotalAmount = (decimal)product.TotalAmount;

                    // Subscription
                    if (product.CourseType_ID == 2)
                    {

                        Subscription subscriptions = await GetLatestSubscription(model.SubscriptionId);

                        // ExceptionLogging.Info(subscriptions.ToJson());
                        if (subscriptions.Status == "active")
                        {
                            // myLogger.Info(subscriptions);

                            MapSubcriptionTransactionDetailsNew(model, product, subscriptions, out TransactionModel TransactionModel, out CoursePurchaseModel purchaseModel);


                            Course_Purchase.UpdateLog(purchaseModel);

                            var CoursePurchased = Course_Purchase.IsCoursePurchased(TransactionModel.Parent_ID, purchaseModel.Log_ID);

                            if (CoursePurchased == "Yes")
                            {
                                int AssignmentPerWeek = Course_Purchase.GetPackQuantity(purchaseModel.Log_ID);
                              //  Parent_Course_Map.UpdateAssignmentPerWeek(TransactionModel.Parent_ID, AssignmentPerWeek, purchaseModel.Log_ID, TransactionModel.SubscriptionId);
                              //  Parent_Course_Map.UpdateCustomerID(Customer_ID, model.Parent_ID, purchaseModel.Log_ID);
                            }

                            resultSet.IsSuccess = 1;
                            resultSet.Message = "Payment Success";
                        }
                        else
                        {
                            string strLog = "Third" + "||" + model.CardName + "||" + model.Email + "||" + subscriptions.CustomerId;
                            strLog += "||" + subscriptions.Status + "||" + model.TokenID + "||" + subscriptions.Id + "||" + subscriptions.Created + "||" + ((subscriptions.Items.Data[0].Plan.AmountDecimal) / 100).ToString();

                            // ExceptionLogging.Info(strLog);
                        }
                    }
                    else
                    {
                        PaymentIntent charge = await GetIntentDetailById(model.IntentId);

                        //charge.Status = "succe";
                        if (charge.Status == "succeeded")
                        {

                            MapOneTimeTransactionDetails(model, product, charge, out TransactionModel TransactionModel, out CoursePurchaseModel purchaseModel);

                            Course_Purchase.UpdateLog(purchaseModel);
                            resultSet.IsSuccess = 1;
                            resultSet.Message = "Payment Success";

                        }

                    }
                }
                //Email Setup

            }
            catch (Stripe.StripeException e)
            {
                string strLogMessage = string.Empty;
                switch (e.StripeError.Error)
                {
                    case "card_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;

                    case "api_connection_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;

                    case "api_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;

                    case "authentication_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;


                    case "invalid_request_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;

                    case "rate_limit_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;

                    case "validation_error":
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;
                    default:
                        strLogMessage = "Home Controller > Charge ";
                        strLogMessage += " In Method (Charge) for exception from Stripe : Code - " + e.StripeError.Code + "; Payment Exception - " + e.StripeError.Message;
                        break;
                }

                resultSet.IsSuccess = 0;
                resultSet.Message = e.StripeError.Type;
                resultSet.Data = e.Message;
                //ExceptionLogging.Error(strLogMessage);
            }
            catch (Exception ex)
            {
                resultSet.IsSuccess = 0;
                resultSet.Message = ex.Message;
                resultSet.Data = ex.Message;
               // ExceptionLogging.Error(ex.Message.ToString());
            }
            return resultSet;
        }

        private void MapOneTimeTransactionDetails(CheckoutViewModel model, ProductViewModel viewModel, PaymentIntent charge, out TransactionModel transModel, out CoursePurchaseModel purchaseModel)
        {
            var courseData = Course_Purchase.GetCourseModelByMultiPurchase_ID(model.MultiPurchase_ID, viewModel.Course_ID);


            transModel = new TransactionModel
            {
                FirstName = model.Parent_Name,
                CardName = model.CardName,
                PaymentStatus = "Completed",
                PayerEmail = model.Email,
                TxnType = "subscr_payment",
                Mc_gross = (charge.Amount / 100).ToString(),// (charge.Lines.Data[0].Price.UnitAmountDecimal / 100).ToString(),//(charge.Lines.Data[0].Price.UnitAmountDecimal / 100).ToString(),

                Log_ID = (int)courseData.Log_ID,
                //SubscriptionId = charge.ChargeId,// charge.Id, //
                Quantity = viewModel.Quantity,
                ProductName = courseData.Course_Name,
                ProductId = viewModel.Product_ID,//               charge.Lines.Data[0].Price.ProductId,
                //PlanId = charge. charge.Lines.Data[0].Price.Id,
                TxnId = charge.Id, //charge.Number,
                InvoiceId = charge.Id,  // intentid
                //InvoiceUrl = charge.HostedInvoiceUrl,
                CreatedDate = charge.Created,
                CustomerID = charge.CustomerId,
                TokenID = model.TokenID,
                Parent_ID = model.Parent_ID,
                Course_ID = (int)courseData.Course_ID,
                AppliedPromoID = model.PurchasePromoCode,
                FullName = model.Parent_Name,
                CourseStartDate = charge.Created,
                CourseEndDate = DateTime.Now.AddMonths(12)
            };
            string strLog = "MapOneTimeTransactionDetails || "
                + transModel.Log_ID + "||" + transModel.Parent_ID + "||" + transModel.Course_ID + "||";
            strLog += transModel.FirstName + "||" + transModel.PayerEmail + "||" + transModel.Mc_gross + "||" + transModel.Status + "||";
            strLog += transModel.InvoiceId + "||"
                //+ transModel.InvoiceUrl + "||" 
                + transModel.CreatedDate + "||";
            strLog += transModel.CustomerID + "||" + transModel.TokenID;

          //  ExceptionLogging.Info(strLog);
            var TXNID = TransactionLogs.CreateTransactionLog(transModel);

            purchaseModel = new CoursePurchaseModel()
            {
                Parent_ID = transModel.Parent_ID,
                Name = transModel.FullName,
                Log_ID = transModel.Log_ID,
                Status = "Payment Completed",
                Txn_Type_Status = transModel.TxnType,
                PurchasePromoCode = transModel.AppliedPromoID
            };
        }


        public async Task<PaymentIntent> GetIntentDetailById(string IntentId)
        {
            PaymentIntentService paymentIntentService = new PaymentIntentService();
            InvoiceService _invoiceService = new InvoiceService();
            try
            {
                // Fetch the PaymentIntent using the client_secret
                PaymentIntent paymentIntent = await paymentIntentService.GetAsync(IntentId);
                Invoice invoice = new Invoice();
                if (paymentIntent != null)
                {
                    //invoice = await _invoiceService.GetAsync(paymentIntent.InvoiceId);
                    //if (invoice == null)
                    //{
                    //    return null;
                    //    //return NotFound(new { success = false, message = "Invoice not found." });
                    //}
                    return paymentIntent;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return null;
                //return StatusCode(500, new { success = false, message = "An error occurred." });
            }
            return null;
        }


        public async Task<Subscription> GetLatestSubscription(string subscriptionId)
        {
            SubscriptionService _subscriptionService = new SubscriptionService();

            try
            {
                //var options = new SubscriptionListOptions
                //{
                //    Customer = customerId,
                //    Limit = 10 // Adjust this if necessary
                //};
                var subscription = await _subscriptionService.GetAsync(subscriptionId);

                if (subscription == null)
                {
                    //return NotFound(new { success = false, message = "No subscriptions found for the given customer ID." });
                }

                return subscription;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception occurred: {ex.Message}");
                //return StatusCode(500, new { success = false, message = "An error occurred." });
                return null;
            }
        }


        private static void MapSubcriptionTransactionDetailsNew(CheckoutViewModel viewModel, ProductViewModel model, Subscription subscriptions, out TransactionModel TransactionModel, out CoursePurchaseModel purchaseModel)
        {
            var courseData = Course_Purchase.GetCourseModelByMultiPurchase_ID(viewModel.MultiPurchase_ID, model.Course_ID);

            var PlanData = subscriptions.Items.FirstOrDefault().Plan;

            TransactionModel = new TransactionModel
            {
                FirstName = viewModel.Parent_Name,
                FullName = viewModel.Parent_Name,
                CardName = viewModel.CardName,
                TokenID = viewModel.SubscriptionId,
                Parent_ID = viewModel.Parent_ID,
                PayerEmail = viewModel.Email,
                PaymentStatus = "Completed",

                TxnType = "subscr_payment",
                Log_ID = (int)courseData.Log_ID,

                ProductId = (PlanData != null) ? PlanData.ProductId : string.Empty,
                Mc_gross = (PlanData != null) ? ((PlanData.AmountDecimal) / 100).ToString() : string.Empty,
                PlanId = (PlanData != null) ? PlanData.Id : string.Empty,
                Interval = (PlanData != null) ? PlanData.Interval : string.Empty,
                // Installments = (PlanData != null) ? PlanData.IntervalCount : 0,

                SubscriptionId = subscriptions.Id,
                InvoiceId = subscriptions.LatestInvoiceId,
                InvoiceUrl = subscriptions.Items.Url,
                CreatedDate = subscriptions.Created,
                CourseStartDate = subscriptions.StartDate,
                CourseEndDate = subscriptions.CancelAt,
                CustomerID = subscriptions.CustomerId,

                Quantity = model.Quantity,
                ProductName = model.Name,
                Course_ID = model.Course_ID,
                AppliedPromoID = model.Promo_Code
            };

            string strLog = "First || " + TransactionModel.Log_ID + "||" + TransactionModel.Parent_ID + "||" + TransactionModel.Course_ID + "||";
            strLog += TransactionModel.FirstName + "||" + TransactionModel.PaymentStatus + "||" + TransactionModel.PayerEmail + "||" + TransactionModel.Mc_gross + "||" + TransactionModel.TxnType + "||";
            strLog += TransactionModel.SubscriptionId + "||" + TransactionModel.ProductId + "||" + TransactionModel.PlanId + "||" + TransactionModel.ProductName + "||";
            strLog += TransactionModel.CustomerID + "||" + TransactionModel.TokenID + "||" + TransactionModel.InvoiceId + "||" + TransactionModel.InvoiceUrl;
            // ExceptionLogging.Info(strLog);

            var TXNID = TransactionLogs.CreateTransactionLog(TransactionModel);

            purchaseModel = new CoursePurchaseModel()
            {
                Parent_ID = TransactionModel.Parent_ID,
                Name = TransactionModel.FullName,
                Log_ID = TransactionModel.Log_ID,
                Status = "Payment Completed",
                Txn_Type_Status = TransactionModel.TxnType,
                PurchasePromoCode = TransactionModel.AppliedPromoID
            };
            purchaseModel.Status = "Payment Completed";
            purchaseModel.Name = TransactionModel.FullName;
            purchaseModel.Txn_Type_Status = TransactionModel.TxnType;


        }

    }
}
