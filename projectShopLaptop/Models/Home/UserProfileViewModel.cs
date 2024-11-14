using projectShopLaptop.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace projectShopLaptop.Models.Home
{
    public class UserProfileViewModel
    {
        public Tbl_User User { get; set; }
        public HomeIndexViewModel HomeIndexData { get; set; }
    }
}