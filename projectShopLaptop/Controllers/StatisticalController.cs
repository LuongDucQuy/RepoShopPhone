using projectShopLaptop.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace projectShopLaptop.Controllers
{
    public class StatisticalController : Controller
    {
        private dbClickShopEntities db = new dbClickShopEntities();
        // GET: Statistical
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult ThongKeSach()
        {
            // Get sales data by date
            var dailySalesStatistics = db.Tbl_Bill
                .GroupBy(b => DbFunctions.TruncateTime(b.ngayDat))  // Use TruncateTime for EF6
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.soTien)
                })
                .OrderBy(g => g.Date)
                .ToList();

            // Prepare the data for the chart
            ViewBag.Dates = dailySalesStatistics.Select(d => d.Date.Value.ToString("yyyy-MM-dd")).ToList();
            ViewBag.Revenue = dailySalesStatistics.Select(d => d.Revenue).ToList();

            return View();
        }


    }
}