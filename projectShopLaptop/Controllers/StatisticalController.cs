using projectShopLaptop.DAL;
using System;
using System.Collections.Generic;
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
    }
}