using projectShopLaptop.DAL;
using projectShopLaptop.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace projectShopLaptop.Controllers
{
    public class detailProductController : Controller
    {
        dbClickShopEntities ctx = new dbClickShopEntities();
        // GET: detailProduct
        public ActionResult proDetail(int id)
        {
            Debug.WriteLine("ID nhận được: " + id);
            var data = ctx.Tbl_Product.SingleOrDefault(p => p.ProductId == id);
            if (data == null)
            {
                return HttpNotFound();
            }

            var result = new ProductDetail
            {
                ProductId = data.ProductId,
                ProductName = data.ProductName,
                Price = data.Price,
                ProductImage = data.ProductImage,
                Quantity = data.Quantity,
                CategoryId = data.CategoryId,
            };

            var categories = ctx.Tbl_Category.Select(c => new
            {
                c.CategoryId,
                c.CategoryName
            }).ToList();

            result.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            // Retrieve reviews for the product
            var reviews = ctx.Reviews.Where(r => r.ProductId == id)
                                     .Select(r => new ReviewViewModel
                                     {
                                         Avatar = r.Tbl_User.avartar,
                                         UserName = r.Tbl_User.UserName,
                                         Rating = r.Rating,
                                         Comment = r.Comment,
                                         ReviewDate = r.ReviewDate
                                     }).ToList();

            // Pass reviews to the view
            ViewBag.Reviews = reviews;

            return View(result);
        }

    }
}