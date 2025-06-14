using EduFeedback.Core.Models;
using EduFeedback.Models;
using EduFeedback.Payment.StripePayment;
using EduFeedback.Web.Helper.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EduFeedback.Web.Controllers
{
    public class StripeController : Controller
    {
        // GET: Stripe
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Charge(CheckoutViewModel paymentModel)
        {
            var cartProductList = CartHelper.GetProductListFromCart();

            // CoursePurchaseModel model = new CoursePurchaseModel();
            StripePayment payment = new StripePayment();
            var result = await payment.MultiCharge(paymentModel, cartProductList);
            if (result.IsSuccess == 1)
            {
                CartHelper.ClearCart();

                return RedirectToAction("Successful", "Services");
            }
            else
            {
                return RedirectToAction("Failure", "Services");
            }
        }

    }
}