using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduFeedback.Core.Models
{
    public class TransactionModel
    {
        public string FirstName { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentType { get; set; }
        public string PayerStatus { get; set; }
        public string PayerEmail { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverEmail { get; set; }
        public string TxnType { get; set; }
        public string TxnId { get; set; }
        public string VerifySign { get; set; }
        public string Mc_gross { get; set; }
        public int Log_ID { get; set; }
        public int Parent_ID { get; set; }
        public int Course_ID { get; set; }
        public string FullName { get; set; }
        public DateTime? CourseEndDate { get; set; }
        public DateTime? CourseStartDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string AmountPaid { get; set; }
        public string DiscountCode { get; set; }
        public string Status { get; set; }
        public string CourseName { get; set; }
        public string AppliedPromoID { get; set; }
        public int CourseId { get; set; }
        public int? Quantity { get; set; }
        public string SubscriptionId { get; set; }
        public string ProductId { get; set; }
        public string PlanId { get; set; }
        public string ProductName { get; set; }
        public string Interval { get; set; }
        public int? Installments { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceUrl { get; set; }
        public string CardName { get; set; }
        public string CustomerID { get; set; }
        public string TokenID { get; set; }
        public string Description { get; set; }

    }
}
