using projectShopLaptop.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using projectShopLaptop.Repository;

namespace projectShopLaptop.Controllers
{
    public class StatisticalController : Controller
    {
        private dbClickShopEntities db = new dbClickShopEntities();
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        public ActionResult ThongKeSach()
        {
            // Lấy danh sách đơn hàng với maTrangThai == 4
            var filteredBills = _unitOfWork.GetRepositoryInstance<Tbl_Bill>()
                .GetProduct()
                .Where(b => b.maTrangThai == 4)
                .ToList();

            // Các thống kê doanh thu hàng ngày
            var dailySalesStatistics = db.Tbl_Bill
                .Where(b => b.maTrangThai == 4)
                .GroupBy(b => DbFunctions.TruncateTime(b.ngayDat))  // Use TruncateTime for EF6
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.soTien)
                })
                .OrderBy(g => g.Date)
                .ToList();


            // Chuẩn bị dữ liệu cho biểu đồ
            ViewBag.Dates = dailySalesStatistics.Select(d => d.Date.Value.ToString("dd-MM-yyyy")).ToList();
            ViewBag.Revenue = dailySalesStatistics.Select(d => d.Revenue).ToList();

            // Thống kê khách hàng mua hàng nhiều nhất
            var topCustomers = filteredBills
                .GroupBy(b => b.user_id)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(b => b.soTien)
                })
                .OrderByDescending(g => g.TotalSpent)
                .Take(10)
                .Join(db.Tbl_User,
                      billGroup => billGroup.CustomerId,
                      user => user.user_id,
                      (billGroup, user) => new
                      {
                          CustomerName = user.Name,
                          TotalSpent = billGroup.TotalSpent
                      })
                .ToList();

            // Lưu vào ViewBag để truyền tới View
            ViewBag.TopCustomerNames = topCustomers.Select(c => c.CustomerName).ToList();
            ViewBag.TopCustomerSpent = topCustomers.Select(c => c.TotalSpent).ToList();

            return View(filteredBills); // Truyền danh sách đơn hàng đã lọc tới View
        }

    }
}