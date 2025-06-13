using EduFeedback.Common;
using EduFeedback.Core.DatabaseContext;
using EduFeedback.Core.Models;
using EduFeedback.Models;
using EduFeedback.Payment.StripePayment;
using EduFeedback.Service.ServiceModels;
using EduFeedback.Service.Services;
using EduFeedback.Web.Helper.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace EduFeedback.Web.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {

            var productFetchList = CartHelper.GetProductListFromCart();
            int displayDiscount = 0;
            //var loggedParentId = CommonHelper.GetLoggedUserID();
            //if (loggedParentId > 0)
            //{
            //    foreach (var p in productFetchList)
            //    {
            //        var isAlreadyPurchased = Parent_Course_Map.CheckServicePurchased(loggedParentId, p.Course_ID);
            //        if (isAlreadyPurchased == "Yes")
            //        {
            //            p.IsAlreadyPurchased = true;
            //        }
            //    }
            //}
            var x = productFetchList.Where(y => y.CourseType_ID == 2).ToList();
            if (x.Count > 0)
            {
                displayDiscount = 1;
            }
            ViewBag.displayDiscount = displayDiscount;

            ViewBag.Name = CommonHelper.GetLoggedUserFName();
            ViewBag.Email = CommonHelper.GetLoggedUserEmail();
            ViewBag.Strip_PK_Key = StripeConstants.Security_PublishKey.ToString();
            return View(productFetchList);
        }
        public async Task<ActionResult> Checkout(string Name, string Email, string PaymentMethodId)

        {
            CheckoutViewModel checkoutVM = new CheckoutViewModel();

            List<CoursePurchaseModel> multiCoursePayment = new List<CoursePurchaseModel>();

            Stripe.StripeConfiguration.ApiKey = StripeConstants.Security_Key;

            var userData = RegisterExistingUser(Name, Email);

            var productList = CartHelper.GetProductListFromCart();

            if (productList.Count() == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var FinalTotalPrice = productList.Sum(x => x.TotalAmount);
            var DiscountedTotalPrice = productList.Sum(x => x.DiscountAmount);
            if (DiscountedTotalPrice > 0)
            {
                checkoutVM.TotalPrice = DiscountedTotalPrice;
            }
            else
            {
                checkoutVM.TotalPrice = FinalTotalPrice;
            }
            // FILL Registration data in model
            var user = new Registration() {
                User_ID=  1,
                FirstName = Name,
                LastName = "",
                Email_ID= Email
            };
             //Registration.GetUserByUserName(Email);

            foreach (var item in productList)
            {
                var product = Course_Master.GetCourseDetail(item.Course_ID);
                if (product != null)
                {
                    CoursePurchaseModel coursePayment = new CoursePurchaseModel();

                    if (item.DiscountAmount > 0)
                    {
                       // coursePayment.IsPromoApplied = 1;
                        coursePayment.PurchasePromoCode = item.Promo_Code;
                        coursePayment.TotalAmount = item.DiscountAmount;
                       // coursePayment.PurchaseDiscountAmount = item.DiscountAmount;
                    }
                    else
                    {
                       // coursePayment.IsPromoApplied = 0;
                        coursePayment.PurchasePromoCode = string.Empty;
                        coursePayment.TotalAmount = product.TotalAmount;
                       // coursePayment.PurchaseDiscountAmount = product.PurchaseDiscountAmount;
                    }

                    coursePayment.Parent_ID = user.User_ID;
                    coursePayment.Name = string.Format("{0} {1}", user.FirstName ?? "", user.LastName ?? "");// Registration.GetFullNameByID(coursePayment.Parent_ID);
                    coursePayment.Email = user.Email_ID;
                   // coursePayment.PackQuantity = item.Quantity;
                    coursePayment.Course_ID = item.Course_ID;
                    coursePayment.Course_Name = product.Course_Name;
                   // coursePayment.CourseTypeID = product.CourseTypeID ?? 0;
                    //CourseTypeID = coursePayment.CourseTypeID;
                    //productId = coursePayment.Product_ID;
                   // coursePayment.Year_ID = product.Year_ID;
                    multiCoursePayment.Add(coursePayment);
                }
            }

            var MultiPurchaseId = Multi_Purchase_Order.SaveMultiPurchaseOrder(user.User_ID, multiCoursePayment.Count(), checkoutVM.TotalPrice);

            multiCoursePayment.ForEach(x =>
            {
                x.MultiPurchase_ID = MultiPurchaseId;
                var Log_ID = Course_Purchase.CreateLog(x);
                checkoutVM.LogIds.Add(Log_ID);
            });

            checkoutVM.Parent_ID = user.User_ID;
            checkoutVM.Parent_Name = string.Format("{0} {1}", user.FirstName, (user.LastName != "" ? user.LastName : ""));
            checkoutVM.Email = user.Email_ID;
            checkoutVM.MultiPurchase_ID = MultiPurchaseId;
            checkoutVM.TotalProductInCart = multiCoursePayment.Count();
            checkoutVM.Strip_PK_Key = StripeConstants.Security_PublishKey.ToString();
            var intentData = await CreatePaymentIntent(checkoutVM.TotalPrice, checkoutVM.Email, checkoutVM.Parent_Name);
            checkoutVM.ClientSecret = intentData.ClientSecret;
            //checkoutVM.PaymentMethodId = PaymentMethodId;
            checkoutVM.SubscriptionId = intentData.SubscriptionId;
            checkoutVM.InvoiceId = intentData.InvoiceId;
          //  checkoutVM.CustomerId = (await CreateCustomer(checkoutVM.Email, checkoutVM.Parent_Name)).Id;
            return View(checkoutVM);
        }

        public async Task<PaymentIndentModel> CreatePaymentIntent(decimal courseFee, String email, String Name)
        {
            Stripe.Customer customer = await CreateCustomer(email, Name);

            var listPayment = CartHelper.GetProductListFromCart();

            long totalAmount = 0;
            var subscriptionItems = new List<Stripe.SubscriptionItemOptions>();
            foreach (var product in listPayment)
            {

                if (product.CourseType_ID == 2)
                {

                    // Add subscription item
                    var price = new Stripe.PriceCreateOptions
                    {
                        //UnitAmount = 0,
                        UnitAmountDecimal = (long)(courseFee * 100),
                        Currency = "gbp",
                        Recurring = new Stripe.PriceRecurringOptions
                        {
                            Interval = "day", // day,month for testing
                        },
                        Product = product.Product_ID,// "prod_QWpJeFW4LDEaH3",

                    };


                    var priceservice = new Stripe.PriceService();
                    Stripe.Price prices = priceservice.Create(price);
                    // var pricesItemOptions = new List<Stripe.SubscriptionItemOptions>();
                    subscriptionItems.Add(new Stripe.SubscriptionItemOptions()
                    {
                        Price = prices.Id,
                    });

                    //var options = new Stripe.SubscriptionCreateOptions
                    //{
                    //    Customer = customer.Id.ToString(),
                    //    Items = new List<Stripe.SubscriptionItemOptions>(),
                    //    // comment this line for Live
                    //    // CancelAt = product.Course_Duration.Value
                    //    //DateTime.Now.AddDays(2),//product.Course_Duration.Value),
                    //    //uncomment for live 
                    //    ProrationBehavior = "none",//  (no expire date)
                    //    PaymentBehavior = "default_incomplete",
                    //    Expand = new List<string> { "latest_invoice.payment_intent" },
                    //};
                    //options.Items.AddRange(pricesItemOptions);

                    ////// For Subscription based Products
                    //SubscriptionService service = new Stripe.SubscriptionService();
                    //Subscription subscription = service.Create(options);

                    //var paymentIntent = subscription.LatestInvoice.PaymentIntent;
                    //subscriptionId = subscription.LatestInvoice.SubscriptionId;
                    //return paymentIntent.ClientSecret;
                }
                else
                {
                    // Calculate total amount for one-time purchases
                    totalAmount += (long)(product.TotalAmount * 100);

                    // Create the invoice item for each product
                    var invoiceItemService = new Stripe.InvoiceItemService();
                    var invoiceItem = invoiceItemService.Create(new Stripe.InvoiceItemCreateOptions
                    {
                        Customer = customer.Id,
                        Amount = (long)(product.TotalAmount * 100), // amount in cents
                        Currency = "gbp",
                        Description = product.Description,
                    });
                }

            }

            string clientSecret = null;
            string invoiceId = null;
            string hostedInvoiceUrl = null;
            string subscriptionId = null;
            if (totalAmount > 0)
            {
                // Create payment intent for one-time purchases
                var paymentIntentService = new Stripe.PaymentIntentService();
                var paymentIntent = paymentIntentService.Create(new Stripe.PaymentIntentCreateOptions
                {
                    Amount = totalAmount, // total amount in cents
                    Currency = "gbp",
                    Customer = customer.Id,
                    PaymentMethodTypes = new List<string> { "card" },
                });
                clientSecret = paymentIntent.ClientSecret;

                //// Create the invoice
                //var invoiceService = new InvoiceService();
                //var invoice = invoiceService.Create(new InvoiceCreateOptions
                //{
                //    Customer = customer.Id,
                //    AutoAdvance = true, // Automatically finalize the invoice
                //    CollectionMethod = "charge_automatically",
                //    //DefaultPaymentMethod = paymentIntent.PaymentMethod.Id,
                //});

                //// Finalize the invoice (optional, if AutoAdvance is true)
                //var finalizedInvoice = invoiceService.FinalizeInvoice(invoice.Id);
                //invoiceId = finalizedInvoice.Id;
                //hostedInvoiceUrl = finalizedInvoice.HostedInvoiceUrl;

                // Pay the invoice
                //var paidInvoice = invoiceService.Pay(finalizedInvoice.Id);
            }
            if (subscriptionItems.Count > 0)
            {
                // Create subscription
                var subscriptionService = new Stripe.SubscriptionService();
                var subscription = subscriptionService.Create(new Stripe.SubscriptionCreateOptions
                {
                    Customer = customer.Id,
                    Items = subscriptionItems,
                    PaymentBehavior = "default_incomplete",
                    Expand = new List<string> { "latest_invoice.payment_intent" },
                    ProrationBehavior = "none",//  (no expire date)
                });

                // var paymentIntent = subscription.LatestInvoice;
                var paymentIntent = subscription.LatestInvoice.PaymentIntent;
                clientSecret = paymentIntent.ClientSecret;
                invoiceId = subscription.LatestInvoice.Id;
                hostedInvoiceUrl = subscription.LatestInvoice.HostedInvoiceUrl;
                subscriptionId = subscription.Id;
            }

            var model = new PaymentIndentModel()
            {
                ClientSecret = clientSecret,
                InvoiceId = invoiceId,
                SubscriptionId = subscriptionId
            };
            return model;
        }

        private static async Task<Stripe.Customer> CreateCustomer(string email, string Name)
        {

            // Create Client
            var customerService = new Stripe.CustomerService();

            // Search for existing customer by email
            var customerListOptions = new Stripe.CustomerListOptions
            {
                Email = email,
                Limit = 1,
            };
            var existingCustomers = await customerService.ListAsync(customerListOptions);
            Stripe.Customer customer;

            if (existingCustomers.Data.Count > 0)
            {
                // Update existing customer with new payment method
                customer = existingCustomers.Data[0];
            }
            else
            {
                // Create a new customer
                var createOptions = new Stripe.CustomerCreateOptions
                {
                    Email = email,
                    Name = Name,
                    //PaymentMethod = model.TokenID,
                    // after paymentMethod initilization use this line
                    //InvoiceSettings = new CustomerInvoiceSettingsOptions { DefaultPaymentMethod = model.PaymentMethodId },
                };

                customer = await customerService.CreateAsync(createOptions);
            }

            return customer;
        }

        private async Task<Registration> RegisterExistingUser(string Name, string Email)
        {
            // Helper helperObj = new Helper();
             // Check if user is already registered
             Registration userData = Registration.GetUserByUserName(Email);
            if (userData == null)
            {
                //New User
                // Generate new password for him
                var getRandomPassword = RandomPassword.Generate(8, 10); ;
                Core.Models.RegistrationModel model = new Core.Models.RegistrationModel() { Email_ID = Email, Password = getRandomPassword, Type = "Parent", Year_ID = 10, FirstName = Name };
                // Register new user
                model.User_ID = await Registration.CreateParent(model);

                if (model.User_ID > 0)
                {
                    try
                    { // Assigned Roles to new user
                        Registration.AssignRole(model.User_ID, model.Type);
                        model.FirstName = Name;
                        model.UserName = model.Email_ID;

                        // Create account 
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password, false);

                        // store visible password in table
                        // Registration.UpdateVisiblePassword(model.UserName, model.Password);

                        if (WebSecurity.Login(model.UserName, model.Password, persistCookie: false))
                        {
                            // Emails: 1. Registration email with credentials

                           // var EmailData = new EmailModel();
                           // string DetailsFormat = string.Format("<p><strong> User: {0}</strong></p></br><p>  <strong>Password: {1}</strong></p>", model.UserName, model.Password);
                          //  EmailData.User_ID = model.User_ID;

                          //  EmailData.ContentCode = "REG";
                         //   DetailsFormat += "</br></br><p>We look forward to working with you and your child to help them achieve academic success.</p>";
                         //   EmailData.AddBody = DetailsFormat;



                           // EmailContent.SendMail(EmailData);

                            userData = Registration.GetUserByUserName(model.Email_ID);
                           SetLoginSession(userData);

                        }
                    }
                    catch (MembershipCreateUserException ex)
                    {
                        String strLogMessage = "Account Controller > Register Existing User > ";
                        strLogMessage += " In Method (POst : RegisterExistingUser) , with message : " + ex.Message;
                       // ExceptionLogging.Error(strLogMessage + " " + ex.StackTrace); ;
                    }

                }
            }
            else
            {  // Existing User
                SetLoginSession(userData);
            }

            return userData;

        }

        private void SetLoginSession(Registration userData)
        {
          //  var Defaultfeatures = Feature.DefaultFeature(userData.User_ID);

            CommonHelper.SaveCurrentOrganisationId((long)userData.OrgnKey);
            CommonHelper.SaveLoggedUserID(userData.User_ID);
          //  if (Defaultfeatures != null)
           //     CommonHelper.SaveLoggedUserRole(Defaultfeatures[0].RoleName);
        }
    }
}