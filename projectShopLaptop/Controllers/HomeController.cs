using PagedList;
using projectShopLaptop.DAL;
using projectShopLaptop.Models.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace projectShopLaptop.Controllers
{
    public class HomeController : Controller
    {
        dbClickShopEntities ctx = new dbClickShopEntities();   
        public ActionResult Index(string search, int? page)
        {
            //if (Session["user"] == null)
            //{
            //    return RedirectToAction("Login");
            //}
            //else
            //{
            HomeIndexViewModel model = new HomeIndexViewModel();

            // Lấy danh sách các danh mục từ cơ sở dữ liệu
            ViewBag.Categories = ctx.Tbl_Category.ToList();

            return View(model.CreateModel(search, 8, page));
            //}
        }
        [HttpGet]
        public ActionResult ProductOfCategory(int id, int? page)
        {
            int pageSize = 10;  
            int pageNumber = (page ?? 1);

            IPagedList<Tbl_Product> products = ctx.Tbl_Product
                .Where(p => p.CategoryId == id)
                .OrderBy(p => p.ProductId)
                .ToPagedList(pageNumber, pageSize);

            return View(new HomeIndexViewModel
            {
                ListOfProduct = products
            });
        }

        //public ActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Login(string user, string password)
        //{
        //    using (var dbContext = new dbClickShopEntities())
        //    {
        //        // Tìm người dùng theo tên đăng nhập
        //        var existingUser = dbContext.Tbl_User
        //                            .FirstOrDefault(u => u.UserName.ToLower() == user.ToLower());

        //        if (existingUser != null)
        //        {
        //            // Mã hóa mật khẩu người dùng nhập vào để so sánh
        //            var hashedPassword = HashPassword(password);

        //            // Kiểm tra mật khẩu đã mã hóa
        //            if (existingUser.Password == hashedPassword)
        //            {
        //                // Đăng nhập thành công với mật khẩu đã mã hóa
        //                SetUserSession(existingUser);
        //                return RedirectBasedOnRole(existingUser.role);
        //            }
        //            // Kiểm tra mật khẩu chưa mã hóa (dành cho trường hợp dữ liệu cũ)
        //            else if (existingUser.Password == password)
        //            {
        //                // Nếu mật khẩu khớp, tiến hành cập nhật mật khẩu đã mã hóa trong cơ sở dữ liệu
        //                existingUser.Password = hashedPassword;
        //                dbContext.SaveChanges();

        //                // Đăng nhập thành công với mật khẩu chưa mã hóa (sau khi cập nhật lại thành mã hóa)
        //                SetUserSession(existingUser);
        //                return RedirectBasedOnRole(existingUser.role);
        //            }
        //            else
        //            {
        //                // Sai mật khẩu
        //                TempData["error"] = "Tài khoản đăng nhập không đúng.";
        //                return View();
        //            }
        //        }
        //        else
        //        {
        //            TempData["error"] = "Tài khoản đăng nhập không đúng.";
        //            return View();
        //        }
        //    }
        //}

        // Hàm thiết lập thông tin phiên đăng nhập
        private void SetUserSession(Tbl_User user)
        {
            Session["user"] = user.UserName;
            Session["role"] = user.role;
        }

        // Hàm chuyển hướng dựa trên vai trò của người dùng
        private ActionResult RedirectBasedOnRole(string role)
        {
            if (role == "Admin")
            {
                return RedirectToAction("customerAccount", "Admin");
            }
            else if (role == "User")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = "Vai trò của người dùng không hợp lệ.";
                return RedirectToAction("Login");
            }
        }


        public ActionResult Logout()
        {
            Session.Remove("user");
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        public ActionResult Registe()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registe(string name, string user, string email, string password, string confirmPassword)
        {
            using (var dbContext = new dbClickShopEntities())
            {
                var existingUser = dbContext.Tbl_User.FirstOrDefault(u => u.UserName == user);
                var existingEmail = dbContext.Tbl_User.FirstOrDefault(u => u.EmailId == email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("UsernameExists", "Tên đăng nhập đã tồn tại.");
                    return View(); 
                }

                if (existingEmail != null)
                {
                    ModelState.AddModelError("EmailExists", "Email này đã đăng kí tài khoản.");
                    return View();
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError("PasswordMismatch", "Mật khẩu và xác nhận mật khẩu không khớp.");
                    return View();
                }

                if (!ModelState.IsValid)
                {
                    
                    return View();
                }

                var hashedPassword = HashPassword(password);

                var newUser = new Tbl_User
                {
                    Name = name,
                    UserName = user,
                    EmailId = email,
                    Password = hashedPassword,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    IsActive = true,
                    role = "User"
                };

                dbContext.Tbl_User.Add(newUser);
                dbContext.SaveChanges();
                return RedirectToAction("Login");
            }
        }

        // Hàm mã hóa mật khẩu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Chuyển mật khẩu thành mảng byte
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển đổi mảng byte thành chuỗi hex
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public ActionResult shoppingCart()
        {
            IEnumerable<Item> cart = Session["cart"] as List<Item> ?? new List<Item>();

            return View(
                new HomeIndexViewModel()
                {
                    items = cart

                }
            ); 
        }

        [HttpPost]
        public ActionResult Checkout(string Name, string Address, string PhoneNumber)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login");
            }

            string username = Session["user"].ToString();
            var user = ctx.Tbl_User.FirstOrDefault(u => u.UserName == username);

            if (user != null)
            {
                // Lấy thông tin giỏ hàng từ session
                var cart = Session["cart"] as List<Item>;

                if (cart != null && cart.Any())
                {
                    // Tạo đối tượng Tbl_Bill
                    var bill = new Tbl_Bill
                    {
                        user_id = user.user_id, // Giả sử ID người dùng là trường khóa chính
                        ngayDat = DateTime.Now,
                        HoTen = Name,
                        diaChi = Address,
                        dienThoai = PhoneNumber,
                        soTien = (decimal)cart.Sum(item => item.Quantity * item.Product.Price),
                        cachThanhToan = "COD", // Hoặc lấy từ form nếu có nhiều phương thức thanh toán
                        cachVanChuyen = "Standard Shipping", // Hoặc lấy từ form
                        maTrangThai = 1, // Ví dụ: 1 là trạng thái 'Đang xử lý'
                        ghiChu = ""
                    };

                    // Lưu vào database
                    ctx.Tbl_Bill.Add(bill);
                    ctx.SaveChanges();

                    // Lưu thay đổi
                    ctx.SaveChanges();

                    // Xóa giỏ hàng sau khi đặt hàng
                    Session["cart"] = null;

                    return RedirectToAction("Index");
                }
            }

            TempData["error"] = "Không thể hoàn thành đặt hàng.";
            return RedirectToAction("ShoppingCart");
        }



        public ActionResult Pay()
        {
            //if (Session["user"] != null)
            //{
            //    string username = Session["user"].ToString();
            //    var user = ctx.Tbl_User.FirstOrDefault(u => u.UserName == username);

            //    if (user != null)
            //    {
            //ViewBag.Name = user.Name;
            //ViewBag.Address = user.Address;
            //ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.Name = "Quy";
            ViewBag.Address = "abc";
            ViewBag.PhoneNumber = "123";
            //    }
            //}

            return View();
        }

        [HttpPost]
        public JsonResult AddToCart(int productId)
        {
            var cart = Session["cart"] as List<Item> ?? new List<Item>();
            var product = ctx.Tbl_Product.Find(productId);

            if (product != null)
            {
                var existingItem = cart.FirstOrDefault(item => item.Product.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new Item()
                    {
                        Product = product,
                        Quantity = 1
                    });
                }

                Session["cart"] = cart;

                // Trả về thông tin sản phẩm để cập nhật giỏ hàng
                return Json(new
                {
                    success = true,
                    product = new
                    {
                        productId = product.ProductId,
                        productName = product.ProductName,
                        productImage = product.ProductImage,
                        quantity = cart.FirstOrDefault(item => item.Product.ProductId == productId)?.Quantity ?? 1
                    }
                });
            }

            return Json(new { success = false });
        }


        public ActionResult AddToCart(int productId, string url)
        {
            if (Session["cart"] == null)
            {
                List<Item> cart = new List<Item>();
                var product = ctx.Tbl_Product.Find(productId);
                cart.Add(new Item()
                {
                    Product = product,
                    Quantity = 1
                });
                Session["cart"] = cart;
            }
            else
            {
                List<Item> cart = (List<Item>)Session["cart"];
                var product = ctx.Tbl_Product.Find(productId);
                var existingItem = cart.FirstOrDefault(item => item.Product.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += 1;
                }
                else
                {
                    cart.Add(new Item()
                    {
                        Product = product,
                        Quantity = 1
                    });
                }
                Session["cart"] = cart;
            }

            if (string.IsNullOrEmpty(url))
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(url);
        }


        public ActionResult DecreaseQty(int productId, string url)
        {
            var cart = (List<Item>)Session["cart"];
            var item = cart.FirstOrDefault(i => i.Product.ProductId == productId);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity -= 1;
            }
            else if (item != null && item.Quantity == 1)
            {
                cart.Remove(item);
            }
            Session["cart"] = cart;
            return Redirect(url);
        }

        [HttpPost]
        public JsonResult RemoveFromCart(int productId)
        {
            var cart = Session["cart"] as List<Item> ?? new List<Item>();

            var itemToRemove = cart.FirstOrDefault(item => item.Product.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            Session["cart"] = cart;

            return Json(new
            {
                success = true,
                cartItemCount = cart.Count
            });
        }


        public ActionResult RemoveCart(int id, string url)
        {
            var cart = Session["cart"] as List<Item>;
            if (cart != null)
            {
                var item = cart.SingleOrDefault(p => p.Product.ProductId == id);
                if (item != null)
                {
                    cart.Remove(item);
                }
                // Cập nhật lại session với giỏ hàng đã thay đổi
                Session["cart"] = cart;
            }
            // Sử dụng url để chuyển hướng chính xác
            return RedirectToAction(url);
        }

        [HttpPost]
        public JsonResult Search(string search)
        {
            // Logic tìm kiếm sản phẩm dựa trên biến search
            var results = ctx.Tbl_Product
                .Where(p => p.ProductName.Contains(search))
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductImage,
                    p.Price
                })
                .ToList();

            return Json(results);
        }

    }
}