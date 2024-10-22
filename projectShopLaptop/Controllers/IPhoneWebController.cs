//using PagedList;
//using projectShopLaptop.DAL;
//using projectShopLaptop.Models.Home;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace projectShopLaptop.Controllers
//{
//    public class IPhoneWebController : Controller
//    {
//        // GET: About
//        public ActionResult Index(string search, int? page)
//        {
//            int pageSize = 10; // Bạn có thể điều chỉnh số lượng sản phẩm trên mỗi trang
//            var model = new HomeIndexViewModel().CreateModel(search, pageSize, page);

//            if (model == null || model.ListOfProduct == null)
//            {
//                model = new HomeIndexViewModel { ListOfProduct = new List<Tbl_Product>().ToPagedList(1, pageSize) };
//            }

//            return View(model);
//        }
//    }
//}