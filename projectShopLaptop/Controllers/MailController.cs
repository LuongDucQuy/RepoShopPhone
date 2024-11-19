using projectShopLaptop.DAL;
using projectShopLaptop.Mail;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace projectShopLaptop.Controllers
{
    public class MailController : Controller
    {
        dbClickShopEntities ctx = new dbClickShopEntities();
        // GET: Mail
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (userId == 0 || code == null)
            {
                return View("Error");
            }
            var user = await ctx.Tbl_User.Where(u => u.user_id == userId).FirstOrDefaultAsync();
            user.confirmEmail = true;
            await ctx.SaveChangesAsync();
            return View("ConfirmEmail" );
        }
    }
}