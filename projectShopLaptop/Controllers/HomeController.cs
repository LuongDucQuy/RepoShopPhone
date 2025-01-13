
using log4net;
using Microsoft.Extensions.Logging;
using PagedList;
using PayPal.Api;
using projectShopLaptop.DAL;
using projectShopLaptop.Mail;
using projectShopLaptop.Models;
using projectShopLaptop.Models.Home;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebGrease;


namespace projectShopLaptop.Controllers
{
    public class HomeController : Controller
    {
        dbClickShopEntities ctx = new dbClickShopEntities();
        public ActionResult Index(string search, int? priceRange, int? category, string sortOrder)
        {
            var products = ctx.Tbl_Product.AsQueryable();

            // Filter by search keyword
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            // Filter by price range
            if (priceRange.HasValue)
            {
                switch (priceRange.Value)
                {
                    case 1: // Under 1 million
                        products = products.Where(p => p.Price < 1000000);
                        break;
                    case 2: // 1 million to 3 million
                        products = products.Where(p => p.Price >= 1000000 && p.Price <= 3000000);
                        break;
                    case 3: // 3 million to 5 million
                        products = products.Where(p => p.Price > 3000000 && p.Price <= 5000000);
                        break;
                    case 4: // Over 5 million
                        products = products.Where(p => p.Price > 5000000);
                        break;
                }
            }

            // Filter by category
            if (category.HasValue)
            {
                products = products.Where(p => p.CategoryId == category.Value);
            }

            // Sort by name or price
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName); // Default sort by name ascending
                    break;
            }

            // Paginate results (Optional)
            var pageNumber = Request.QueryString["page"] != null ? int.Parse(Request.QueryString["page"]) : 1;
            var pageSize = 8; // Number of products per page
            var model = new HomeIndexViewModel
            {
                ListOfProduct = products.ToPagedList(pageNumber, pageSize)
            };

            ViewBag.Categories = ctx.Tbl_Category.ToList();
            ViewBag.CurrentSort = sortOrder;

            return View(model);
        }
        [HttpGet]
        public ActionResult ProductOfCategory(int id, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            IPagedList<Tbl_Product> products = ctx.Tbl_Product
                .Where(p => p.CategoryId == id && p.IsActive==true)
                .OrderBy(p => p.ProductId)
                .ToPagedList(pageNumber, pageSize);

            return View(new HomeIndexViewModel
            {
                ListOfProduct = products
            });
        }

        public ActionResult Login()
        {
            // Khởi tạo đối tượng theo dõi số lần đăng nhập sai nếu chưa có
            if (Session["FailedLoginAttempts"] == null)
            {
                Session["FailedLoginAttempts"] = new Dictionary<string, int>();
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string user, string password)
        {
            using (var dbContext = new dbClickShopEntities())
            {
                // Lấy danh sách số lần đăng nhập sai từ Session
                var failedAttempts = Session["FailedLoginAttempts"] as Dictionary<string, int>;

                // Tìm người dùng trong cơ sở dữ liệu
                var existingUser = dbContext.Tbl_User.FirstOrDefault(u => u.UserName.ToLower() == user.ToLower());

                if (existingUser != null)
                {
                    // Kiểm tra nếu email chưa được xác nhận
                    if (existingUser.confirmEmail == false)
                    {
                        ViewBag.ErrorMessage = "Email của bạn chưa được xác nhận. Vui lòng kiểm tra email hoặc yêu cầu gửi lại.";
                        ViewBag.ResendEmail = true;
                        ViewBag.UserEmail = existingUser.EmailId; // Để tạo liên kết gửi lại email xác nhận
                        return View(existingUser); // Giữ người dùng lại trang Login 
                    }

                    // Kiểm tra số lần đăng nhập sai cho tài khoản hiện tại
                    if (failedAttempts.ContainsKey(user) && failedAttempts[user] >= 3 || existingUser.IsActive == false)
                    {
                        existingUser.IsActive = false;
                        dbContext.SaveChanges();
                        ViewBag.ErrorMessage = "Tài khoản này đã nhập sai quá 3 lần. Vui lòng thử lại sau hoặc đặt lại mật khẩu.";
                        ViewBag.ResetPasswordLink = Url.Action("ForgotPassword", "Home");
                        return View();
                    }

                    // Kiểm tra mật khẩu chưa mã hóa trước
                    if (existingUser.Password == password || existingUser.Password == HashPassword(password))
                    {
                        // Đặt lại số lần đăng nhập sai cho tài khoản này
                        if (failedAttempts.ContainsKey(user))
                        {
                            failedAttempts.Remove(user);
                        }

                        // Thiết lập phiên người dùng và chuyển hướng
                        SetUserSession(existingUser);
                        return RedirectBasedOnRole(existingUser.role);
                    }
                }

                // Xử lý trường hợp đăng nhập sai
                if (failedAttempts.ContainsKey(user))
                {
                    failedAttempts[user]++;
                }
                else
                {
                    failedAttempts[user] = 1;
                }

                ViewBag.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }
        }


        [AllowAnonymous]
        public async Task<ActionResult> ResendConfirmationEmail(string email)
        {
            var user = await ctx.Tbl_User.Where(u => u.EmailId == email).FirstOrDefaultAsync();
            if (user != null && !user.confirmEmail)
            {
                string code = Guid.NewGuid().ToString();

                // Tạo đường dẫn xác nhận
                var callbackUrl = Url.Action("ConfirmEmail", "Mail", new { userId = user.user_id, code = code }, protocol: Request.Url.Scheme);

                // Gửi email xác nhận
                SendMail.SendEmail(user.EmailId, "Confirm your account", "Please click <a href=\"" + callbackUrl + "\">here</a> to confirm your account.", "");
            }
            return View("EmailCheck");
        }

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

                // Kiểm tra mật khẩu ít nhất 6 ký tự
                if (password.Length < 6)
                {
                    ModelState.AddModelError("PasswordLength", "Mật khẩu phải có ít nhất 6 ký tự.");
                    return View();
                }

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
                // Tạo mã xác nhận
                string code = Guid.NewGuid().ToString();

                // Tạo đường dẫn xác nhận
                var callbackUrl = Url.Action("ConfirmEmail", "Mail", new { userId = newUser.user_id, code = code }, protocol: Request.Url.Scheme);

                // Gửi email xác nhận
                SendMail.SendEmail(newUser.EmailId, "Confirm your account", "Please click <a href=\"" + callbackUrl + "\">here</a> to confirm your account.", "");

                // Thông báo gửi thành công
                ViewBag.ThongBao = "Chúng tôi đã gửi cho bạn một email để xác thực. Vui lòng kiểm tra!";

                return View("EmailCheck");
            }
        }

        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login");
            }

            string username = Session["user"].ToString();
            var user = ctx.Tbl_User.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                TempData["error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index");
            }

            var viewModel = new UserProfileViewModel
            {
                User = user,
                HomeIndexData = new HomeIndexViewModel() // Khởi tạo dữ liệu nếu cần
            };

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Profile(Tbl_User updatedUser, HttpPostedFileBase Avatar)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login");
            }

            string username = Session["user"].ToString();
            var user = ctx.Tbl_User.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                TempData["error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index");
            }

            bool isUpdated = false;

            // Kiểm tra và cập nhật thông tin nếu có sự thay đổi
            if (!string.IsNullOrEmpty(updatedUser.Name) && updatedUser.Name != user.Name)
            {
                user.Name = updatedUser.Name;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(updatedUser.EmailId) && updatedUser.EmailId != user.EmailId)
            {
                user.EmailId = updatedUser.EmailId;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(updatedUser.Address) && updatedUser.Address != user.Address)
            {
                user.Address = updatedUser.Address;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(updatedUser.PhoneNumber) && updatedUser.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = updatedUser.PhoneNumber;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(updatedUser.Password) && updatedUser.Password != user.Password)
            {
                user.Password = HashPassword(updatedUser.Password);
                isUpdated = true;
            }

            // Xử lý upload Avatar
            if (Avatar != null && Avatar.ContentLength > 0)
            {
                string fileName = Path.GetFileName(Avatar.FileName);
                string path = Path.Combine(Server.MapPath("~/img"), fileName);
                Avatar.SaveAs(path);
                user.avartar = "~/img/" + fileName;
                isUpdated = true;
            }

            // Nếu có thay đổi, cập nhật và lưu vào cơ sở dữ liệu
            if (isUpdated)
            {
                user.ModifiedOn = DateTime.Now;
                ctx.SaveChanges();
                TempData["successMessage"] = "Cập nhật thành công!";
            }
            else
            {
                TempData["info"] = "Không có thay đổi nào để cập nhật.";
            }

            return RedirectToAction("Index");
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

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            using (var dbContext = new dbClickShopEntities())
            {
                var user = await dbContext.Tbl_User.FirstOrDefaultAsync(u => u.EmailId == email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                    return View();
                }

                string code = Guid.NewGuid().ToString();
                var callbackUrl = Url.Action("ResetPassword", "Home", new { email = user.EmailId }, protocol: Request.Url.Scheme);

                SendMail.SendEmail(user.EmailId, "Reset Password", $"Please reset your password by clicking <a href=\"{callbackUrl}\">here</a>.", "");

                ViewBag.Message = "Liên kết đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra!";
                return View("EmailCheck");
            }
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                // Xử lý lỗi nếu thiếu email
                return RedirectToAction("Index");
            }

            // Truyền email vào View để sử dụng cho việc hiển thị
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(string email, string password, string confirmPassword)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Email = email;  // Giữ lại email khi có lỗi
                return View();
            }

            // Kiểm tra nếu mật khẩu và xác nhận mật khẩu khớp nhau
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận mật khẩu không khớp.");
                ViewBag.Email = email;  // Giữ lại email khi mật khẩu không khớp
                return View();
            }

            using (var dbContext = new dbClickShopEntities())
            {
                var user = await dbContext.Tbl_User.FirstOrDefaultAsync(u => u.EmailId == email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                    ViewBag.Email = email;  // Giữ lại email nếu không tìm thấy người dùng
                    return View();
                }

                // Cập nhật mật khẩu
                user.Password = HashPassword(password);  // Giả sử bạn có phương thức hash mật khẩu
                user.IsActive = true;
                await dbContext.SaveChangesAsync();

                return RedirectToAction("Login", "Home");
            }
        }


        public ActionResult shoppingCart()
        {
            IEnumerable<Models.Home.Item> cart = Session["cart"] as List<Models.Home.Item> ?? new List<Models.Home.Item>();

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
                var cart = Session["cart"] as List<Models.Home.Item>;

                if (cart != null && cart.Any())
                {
                    decimal totalAmount = 0;

                    // Kiểm tra xem tổng tiền sau giảm giá có trong session không
                    if (Session["discountedTotal"] != null)
                    {
                        // Sử dụng tổng tiền đã giảm giá
                        totalAmount = (decimal)Session["discountedTotal"];
                    }
                    else
                    {
                        // Tính tổng tiền từ sản phẩm trong giỏ nếu không có giảm giá
                        foreach (var item in cart)
                        {
                            var product = ctx.Tbl_Product.FirstOrDefault(p => p.ProductId == item.Product.ProductId);

                            if (product == null || product.Quantity < item.Quantity)
                            {
                                TempData["error"] = $"Sản phẩm '{item.Product.ProductName}' không đủ số lượng trong kho.";
                                return RedirectToAction("ShoppingCart");
                            }

                            // Tính tổng tiền
                            totalAmount += item.Quantity * (product.Price ?? 0);
                        }
                    }

                    // Tạo hóa đơn
                    var bill = new Tbl_Bill
                    {
                        user_id = user.user_id,
                        ngayDat = DateTime.Now,
                        HoTen = Name,
                        diaChi = Address,
                        dienThoai = PhoneNumber,
                        soTien = totalAmount, // Lưu tổng tiền sau khi áp dụng giảm giá
                        cachThanhToan = "COD",
                        cachVanChuyen = "Standard Shipping",
                        maTrangThai = 1, // Ví dụ: 1 là trạng thái 'Đang xử lý'
                        ghiChu = ""
                    };

                    // Lưu hóa đơn và cập nhật kho
                    foreach (var item in cart)
                    {
                        var product = ctx.Tbl_Product.FirstOrDefault(p => p.ProductId == item.Product.ProductId);

                        if (product == null || product.Quantity < item.Quantity)
                        {
                            TempData["error"] = $"Sản phẩm '{item.Product.ProductName}' không đủ số lượng trong kho.";
                            return RedirectToAction("ShoppingCart");
                        }

                        // Giảm số lượng sản phẩm trong kho
                        product.Quantity -= item.Quantity;
                        if (product.Quantity == 0)
                        {
                            product.IsActive = false; // Cập nhật trạng thái nếu hết hàng
                        }
                    }

                    // Lưu hóa đơn vào database
                    ctx.Tbl_Bill.Add(bill);
                    ctx.SaveChanges();

                    // Xóa giỏ hàng và thông tin giảm giá
                    Session["cart"] = null;
                    Session["discountedTotal"] = null;

                    TempData["successMessage"] = "Đặt hàng thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "Giỏ hàng trống.";
                }
            }
            else
            {
                TempData["error"] = "Không thể tìm thấy thông tin người dùng.";
            }

            return RedirectToAction("ShoppingCart");
        }



        public ActionResult Pay()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["user"] != null)
            {
                string username = Session["user"].ToString();
                var user = ctx.Tbl_User.FirstOrDefault(u => u.UserName == username);

                if (user != null)
                {
                    ViewBag.Name = user.Name;
                    ViewBag.Address = user.Address;
                    ViewBag.PhoneNumber = user.PhoneNumber;
                }
            }

            return View();
        }

        [HttpPost]
        public JsonResult AddToCart(int productId)
        {
            var cart = Session["cart"] as List<Models.Home.Item> ?? new List<Models.Home.Item>();
            var product = ctx.Tbl_Product.Find(productId);

            if (product != null)
            {
                var existingItem = cart.FirstOrDefault(item => item.Product.ProductId == productId);

                if (existingItem != null)
                {
                    // Kiểm tra số lượng tồn kho
                    if (existingItem.Quantity + 1 > product.Quantity)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Số lượng sản phẩm vượt quá số lượng tồn kho."
                        });
                    }

                    existingItem.Quantity++;
                }
                else
                {
                    // Kiểm tra tồn kho trước khi thêm mới
                    if (product.Quantity <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Sản phẩm đã hết hàng."
                        });
                    }

                    cart.Add(new Models.Home.Item()
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

            return Json(new { success = false, message = "Sản phẩm không tồn tại." });
        }

        public ActionResult AddToCart(int productId, string url)
        {
            var product = ctx.Tbl_Product.Find(productId);

            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return Redirect(string.IsNullOrEmpty(url) ? Url.Action("Index", "Home") : url);
            }

            var cart = Session["cart"] as List<Models.Home.Item> ?? new List<Models.Home.Item>();
            var existingItem = cart.FirstOrDefault(item => item.Product.ProductId == productId);

            if (existingItem != null)
            {
                // Kiểm tra số lượng tồn kho
                if (existingItem.Quantity + 1 > product.Quantity)
                {
                    TempData["Error"] = "Số lượng sản phẩm vượt quá số lượng tồn kho.";
                    return Redirect(string.IsNullOrEmpty(url) ? Url.Action("Index", "Home") : url);
                }

                existingItem.Quantity++;
            }
            else
            {
                // Kiểm tra tồn kho trước khi thêm mới
                if (product.Quantity <= 0)
                {
                    TempData["Error"] = "Sản phẩm đã hết hàng.";
                    return Redirect(string.IsNullOrEmpty(url) ? Url.Action("Index", "Home") : url);
                }

                cart.Add(new Models.Home.Item()
                {
                    Product = product,
                    Quantity = 1
                });
            }

            Session["cart"] = cart;

            TempData["Success"] = "Thêm sản phẩm vào giỏ hàng thành công!";
            return Redirect(string.IsNullOrEmpty(url) ? Url.Action("Index", "Home") : url);
        }


        public ActionResult DecreaseQty(int productId, string url)
        {
            var cart = (List<Models.Home.Item>)Session["cart"];
            var item = cart.FirstOrDefault(i => i.Product.ProductId == productId);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity -= 1;
            }
            else if (item != null && item.Quantity == 1)
            {
                item.Quantity = 1;
            }
            Session["cart"] = cart;
            return Redirect(url);
        }

        [HttpPost]
        public JsonResult RemoveFromCart(int productId)
        {
            var cart = Session["cart"] as List<Models.Home.Item> ?? new List<Models.Home.Item>();

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
            var cart = Session["cart"] as List<Models.Home.Item>;
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

        public ActionResult LichSuDonHang()
        {
            // Kiểm tra nếu người dùng đã đăng nhập
            if (Session["user"] == null)
            {
                return RedirectToAction("Login");
            }

            // Lấy tên người dùng từ session
            string username = Session["user"].ToString();

            using (var dbContext = new dbClickShopEntities())
            {
                // Lấy thông tin người dùng từ cơ sở dữ liệu
                var user = dbContext.Tbl_User.FirstOrDefault(u => u.UserName == username);

                if (user != null)
                {
                    // Lấy danh sách các đơn hàng của người dùng
                    var orders = dbContext.Tbl_Bill
                        .Where(b => b.user_id == user.user_id) // lọc đơn hàng của người dùng
                        .OrderByDescending(b => b.ngayDat) // sắp xếp theo ngày đặt hàng
                        .ToList();

                    // Trả về view với danh sách đơn hàng
                    return View(orders);
                }
                else
                {
                    TempData["error"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        public ActionResult CancelOrder(int billId)
        {
            // Lấy thông tin đơn hàng từ CSDL
            var bill = ctx.Tbl_Bill.FirstOrDefault(b => b.IDBill == billId);

            if (bill != null)
            {
                // Kiểm tra trạng thái đơn hàng
                if (bill.maTrangThai == 1 || bill.maTrangThai == 2 || bill.maTrangThai == 3)
                {
                    // Lấy danh sách sản phẩm trong hóa đơn
                    var billDetails = ctx.Tbl_Bill.Where(d => d.IDBill == billId).ToList();

                    // Cộng lại số lượng sản phẩm vào kho
                    foreach (var detail in billDetails)
                    {
                        var product = ctx.Tbl_Product.FirstOrDefault(p => p.ProductId == detail.ProductId);
                        if (product != null)
                        {
                            product.Quantity += 1; // Tăng số lượng tồn kho
                        }
                    }

                    // Cập nhật trạng thái đơn hàng thành "Đã hủy"
                    bill.maTrangThai = 5; // 5 = Đã hủy (mã trạng thái bạn tự định nghĩa)
                    ctx.SaveChanges();
                    // Thông báo hủy thành công
                    TempData["successMessage"] = "Đơn hàng đã được hủy thành công.";
                }
                else
                {
                    // Thông báo không thể hủy đơn hàng
                    TempData["Message"] = "Không thể hủy đơn hàng này vì đã được xử lý.";
                }
            }
            else
            {
                // Thông báo đơn hàng không tồn tại
                TempData["Message"] = "Đơn hàng không tồn tại.";
            }

            // Quay lại trang lịch sử đơn hàng của khách hàng
            return RedirectToAction("LichSuDonHang");
        }


        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult VnPay(double Tongtien, string Name, string Address, string PhoneNumber)
        {
            // Kiểm tra nếu người dùng chưa đăng nhập
            if (Session["user"] == null)
            {
                return RedirectToAction("Login");
            }

            string username = Session["user"].ToString();
            var user = ctx.Tbl_User.FirstOrDefault(u => u.UserName == username);

            if (user != null)
            {
                // Lấy thông tin giỏ hàng từ session
                var cart = Session["cart"] as List<Models.Home.Item>;

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
                        cachThanhToan = "VNPay", // Phương thức thanh toán là VNPAY
                        cachVanChuyen = "Standard Shipping", // Hoặc lấy từ form
                        maTrangThai = 1, // Ví dụ: 1 là trạng thái 'Đang xử lý'
                        ghiChu = "",
                    };

                    // Lưu vào database
                    ctx.Tbl_Bill.Add(bill);
                    ctx.SaveChanges();

                    // Chuyển hướng tới trang thanh toán VNPAY
                    string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"];
                    string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
                    string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
                    string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

                    if (string.IsNullOrEmpty(vnp_Returnurl) || string.IsNullOrEmpty(vnp_Url) ||
                        string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
                    {
                        throw new Exception("Cấu hình VNPAY không hợp lệ trong AppSettings.");
                    }

                    // Khởi tạo thông tin đơn hàng
                    OrderInfo order = new OrderInfo
                    {
                        OrderId = DateTime.Now.Ticks, // Mã giao dịch giả lập
                        Amount = (long)Tongtien,     // Số tiền thanh toán (VND)
                        Status = "0",                // Trạng thái giao dịch
                        CreatedDate = DateTime.Now   // Ngày tạo giao dịch
                    };

                    // Khởi tạo thư viện VNPAY
                    VnPayLibrary vnpay = new VnPayLibrary();

                    // Thêm các tham số bắt buộc
                    vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString()); // Nhân 100 để chuyển sang đơn vị nhỏ nhất
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang: {order.OrderId}");
                    vnpay.AddRequestData("vnp_OrderType", "other"); // Loại giao dịch
                    vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                    vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                    vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());
                    vnpay.AddRequestData("vnp_Version", "2.1.0");

                    // Tạo URL thanh toán
                    string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                    TempData["successMessage"] = "Đặt hàng thành công!";
                    // Chuyển hướng tới URL thanh toán
                    Response.Redirect(paymentUrl);
                }
            }
            return RedirectToAction("ShoppingCart");
        }

        [HttpPost]
        public JsonResult SubmitReview(int ProductId, int Rating, string Comment)
        {
            try
            {
                // Kiểm tra nếu người dùng đã đăng nhập
                if (Session["user"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập trước khi đánh giá." });
                }

                // Lấy tên người dùng từ session
                string username = Session["user"].ToString();

                using (var dbContext = new dbClickShopEntities())
                {
                    // Lấy thông tin người dùng từ cơ sở dữ liệu
                    var user = dbContext.Tbl_User.FirstOrDefault(u => u.UserName == username);

                    if (user != null)
                    {
                        // Lấy user_id từ đối tượng người dùng
                        int userId = user.user_id;

                        // Kiểm tra xem sản phẩm có tồn tại hay không
                        var product = dbContext.Tbl_Product.FirstOrDefault(p => p.ProductId == ProductId);

                        if (product == null)
                        {
                            return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                        }

                        // Kiểm tra phạm vi Rating (nếu cần)
                        if (Rating < 1 || Rating > 5)
                        {
                            return Json(new { success = false, message = "Đánh giá phải trong phạm vi từ 1 đến 5" });
                        }

                        // Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
                        var existingReview = dbContext.Reviews
                            .FirstOrDefault(r => r.ProductId == ProductId && r.user_id == userId);

                        if (existingReview != null)
                        {
                            return Json(new { success = false });
                        }

                        // Tạo đối tượng Review mới
                        var review = new Review
                        {
                            ProductId = ProductId,
                            user_id = userId,
                            Rating = (byte)Rating,  // Chuyển Rating thành byte
                            Comment = Comment,
                            ReviewDate = DateTime.Now
                        };

                        // Thêm đánh giá vào cơ sở dữ liệu
                        dbContext.Reviews.Add(review);
                        dbContext.SaveChanges();

                        // Trả về kết quả thành công
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                    }
                }
            }
            catch (Exception ex)
            {
                // Trả về kết quả lỗi nếu có
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult DeleteReview(int billId)
        {
            var order = ctx.Tbl_Bill.FirstOrDefault(o => o.IDBill == billId);

                // Kiểm tra nếu đơn hàng đã có đánh giá
                var review = ctx.Reviews.FirstOrDefault(r => r.ProductId == order.ProductId);

                if (review != null)
                {
                    ctx.Reviews.Remove(review);  // Xóa đánh giá
                    ctx.SaveChanges();
                    TempData["successMessage"] = "Đánh giá đã được xóa thành công!";
                }
                else
                {
                    TempData["successMessage"] = "Đơn hàng chưa có đánh giá để xóa.";
                }

            return RedirectToAction("LichSuDonHang"); // Quay lại trang lịch sử đơn hàng
        }

        public ActionResult Voucher()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ClaimVoucher(string email)
        {
            using (var dbContext = new dbClickShopEntities())
            {
                // Kiểm tra email tồn tại
                var existingUser = dbContext.Tbl_User.FirstOrDefault(u => u.EmailId == email);
                if (existingUser == null)
                {
                    return Json(new { success = false, message = "Email này chưa được đăng ký." });
                }

                // Lấy voucher còn hiệu lực
                var currentDate = DateTime.Now;
                var voucher = dbContext.Tbl_KhuyenMai
                                       .FirstOrDefault(v => v.DateStart <= currentDate &&
                                                            v.DateEnd >= currentDate &&
                                                            v.SoLuong > 0);

                if (voucher == null)
                {
                    return Json(new { success = false, message = "Hiện tại không có voucher nào khả dụng." });
                }

                // Gửi email chứa mã voucher
                string voucherCode = voucher.VoucherCode;
                string subject = "Mã Voucher của bạn";
                string body = $"Chào bạn, <br/> Mã voucher của bạn là: <strong>{voucherCode}</strong><br/>" +
                              "Hãy sử dụng mã này để được giảm giá tại cửa hàng của chúng tôi.";
                bool emailSent = SendMail.SendEmail(email, subject, body, "");

                if (emailSent)
                {
                    // Giảm số lượng voucher trong cơ sở dữ liệu
                    voucher.SoLuong--;
                    dbContext.SaveChanges();

                    return Json(new { success = true, message = "Mã voucher đã được gửi đến email của bạn!" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại." });
                }
            }
        }

        [HttpPost]
        public JsonResult ApplyPromoCode(string promoCode)
        {
            using (var dbContext = new dbClickShopEntities())
            {
                // Kiểm tra mã giảm giá có tồn tại và còn hiệu lực
                var currentDate = DateTime.Now;
                var voucher = dbContext.Tbl_KhuyenMai
                                       .FirstOrDefault(v => v.VoucherCode == promoCode &&
                                                            v.DateStart <= currentDate &&
                                                            v.DateEnd >= currentDate &&
                                                            v.SoLuong > 0);
                if (voucher == null)
                {
                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hiệu lực." });
                }

                // Lấy giỏ hàng từ Session
                var cart = Session["cart"] as List<Models.Home.Item>;
                if (cart == null)
                {
                    return Json(new { success = false, message = "Giỏ hàng trống." });
                }

                // Tính lại tổng tiền sau giảm giá
                decimal total = (decimal)cart.Sum(item => item.Quantity * item.Product.Price);
                decimal discountAmount = total * 0.05m;
                decimal discountedTotal = total - discountAmount;
                Session["discountedTotal"] = discountedTotal;
                dbContext.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Đơn hàng của bạn được giảm 5%.",
                    totalAfterDiscount = discountedTotal.ToString("C0", new System.Globalization.CultureInfo("vi-VN"))
                });
            }
        }
    }
}
