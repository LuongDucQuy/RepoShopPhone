using PagedList;
using projectShopLaptop.DAL;
using projectShopLaptop.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using System.Drawing.Printing;

namespace projectShopLaptop.Models.Home
{
    public class HomeIndexViewModel
    {
        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        dbClickShopEntities context = new dbClickShopEntities();
        public IPagedList<Tbl_Product> ListOfProduct { get; set; }
        public IEnumerable<Item> items { get; set; }


        public HomeIndexViewModel CreateModel(string search, int pageSize, int? page)
        {
            SqlParameter[] param = new SqlParameter[]
            {
        new SqlParameter("@search", search ?? (object)DBNull.Value)
            };

            var productList = context.Database.SqlQuery<Tbl_Product>("GetBySearch @search", param).ToList();


            // Kiểm tra dữ liệu productList có bị null không
            if (productList == null)
            {
                productList = new List<Tbl_Product>(); // Khởi tạo danh sách trống nếu không có dữ liệu
            }

            IPagedList<Tbl_Product> data = productList.ToPagedList(page ?? 1, pageSize);

            return new HomeIndexViewModel
            {
                ListOfProduct = data
            };

        }

    }
}