using EduFeedback.Models;
using EduFeedback.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace EduFeedback.Web.Helper.Cart
{
    public class CartHelper
    {
        public static void AddProductToCart(int courseId = 0, int assignmentPerWeek = 0, int examDateId = 0, string pExamDateTime = "")
        {
            var courseModel = Core.DatabaseContext.Course_Master.GetCourseDetail(courseId);
            decimal? updatedPrice = (decimal)7.99;// Course_Master.GetCoursePriceAsPerAssignment(courseId, assignmentPerWeek);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Course_ID = (int)courseId,
                Name = courseModel.Course_Name,
                Product_ID = courseModel.Product_ID,
                CourseType_ID = courseModel.CourseTypeID,
                Quantity = 1,
                AssignmentPerWeek = assignmentPerWeek,
                Description = string.Format("Assignment per week - {0}", assignmentPerWeek),
                IsSubscription = false,
                TotalAmount = updatedPrice ?? 0,
                ExamDateId = examDateId,
                ExamDateTime = pExamDateTime
            };
            if (productViewModel.CourseType_ID == 2)
                productViewModel.IsSubscription = true;

            var cart = HttpContext.Current.Session["cart"];
            if (cart == null)
            {
                List<ProductViewModel> li = new List<ProductViewModel>();

                li.Add(productViewModel);

                byte[] bytes = null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, li);
                    bytes = ms.ToArray();
                }
                HttpContext.Current.Session["cart"] = bytes;
                HttpContext.Current.Session["count"] = 1;
            }
            else
            {
                var products = new List<ProductViewModel>();

                BinaryFormatter formatter = new BinaryFormatter();

                using (MemoryStream ms = new MemoryStream((byte[])cart))
                {
                    products = (List<ProductViewModel>)formatter.Deserialize(ms);
                }
                bool prod_exists = false;
                var product_matched = products.Where(p => p.Course_ID == productViewModel.Course_ID).FirstOrDefault();
                if (product_matched != null)
                {
                    prod_exists = true;
                    products.Remove(product_matched);
                    products.Add(productViewModel);
                }

                if (!prod_exists)
                {
                    products.Add(productViewModel);
                }

                byte[] bytes = null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, products);
                    bytes = ms.ToArray();
                }

                HttpContext.Current.Session["cart"] = bytes;
                HttpContext.Current.Session["count"] = products.Count();
            }
        }
        public static void RemoveFromCart(int courseId)
        {
            var cart = HttpContext.Current.Session["cart"];
            if (cart != null)
            {
                var products = new List<ProductViewModel>();

                BinaryFormatter formatter = new BinaryFormatter();

                using (MemoryStream ms = new MemoryStream((byte[])cart))
                {
                    products = (List<ProductViewModel>)formatter.Deserialize(ms);
                }

                var product_matched = new ProductViewModel();
                product_matched = products.Where(p => p.Course_ID == courseId).FirstOrDefault();
                if (product_matched != null)
                {
                    products.Remove(product_matched);
                }

                byte[] bytes = null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, products);
                    bytes = ms.ToArray();
                }

                HttpContext.Current.Session["cart"] = bytes;
                HttpContext.Current.Session["count"] = products.Count();
            }
        }

        public static List<ProductViewModel> GetProductListFromCart()
        {

            var cart = HttpContext.Current.Session["cart"];

            var products = new List<ProductViewModel>();

            if (cart != null)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream((byte[])cart))
                {
                    products = (List<ProductViewModel>)formatter.Deserialize(ms);
                }
            }
            // Discount Calculation
            // var discountList = PromoCodeDataAccess.GetInternalPromos();

            //var discountAvailed = discountList.Select(o => new { o.Discount_On_TotalProduct, o.Discount_Percent })
            //   .Where(x => x.Discount_On_TotalProduct == products.Where(y => y.CourseType_ID == 2).Count())
            //   .FirstOrDefault();

            var result = products.Select(x =>
            {
                //if (discountAvailed != null && x.CourseType_ID == 2)
                //{
                //    x.DiscountAmount = x.TotalAmount - (x.TotalAmount / 100) * (int)discountAvailed.Discount_Percent;
                //    x.DiscountPercent = (int)discountAvailed.Discount_Percent;
                //}
                //else
                //{
                x.DiscountAmount = x.TotalAmount;
                x.DiscountPercent = 0;
                // }
                return x;

            }).ToList();

            return result;
        }
        public static void ClearCart()
        {
            List<ProductViewModel> li = new List<ProductViewModel>();
            byte[] bytes = null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, li);
                bytes = ms.ToArray();
            }
            HttpContext.Current.Session["cart"] = bytes;
            HttpContext.Current.Session["count"] = 0;
        }

    }
}