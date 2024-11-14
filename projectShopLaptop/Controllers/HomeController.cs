using PagedList;
using PayPal.Api;
using projectShopLaptop.DAL;
using projectShopLaptop.Models;
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

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string user, string password)
        {
            using (var dbContext = new dbClickShopEntities())
            {
                // Tìm người dùng theo tên đăng nhập
                var existingUser = dbContext.Tbl_User
                                    .FirstOrDefault(u => u.UserName.ToLower() == user.ToLower());

                if (existingUser != null)
                {
                    // Mã hóa mật khẩu người dùng nhập vào để so sánh
                    var hashedPassword = HashPassword(password);

                    // Kiểm tra mật khẩu đã mã hóa
                    if (existingUser.Password == hashedPassword)
                    {
                        // Đăng nhập thành công với mật khẩu đã mã hóa
                        SetUserSession(existingUser);
                        return RedirectBasedOnRole(existingUser.role);
                    }
                    // Kiểm tra mật khẩu chưa mã hóa (dành cho trường hợp dữ liệu cũ)
                    else if (existingUser.Password == password)
                    {
                        // Nếu mật khẩu khớp, tiến hành cập nhật mật khẩu đã mã hóa trong cơ sở dữ liệu
                        existingUser.Password = hashedPassword;
                        dbContext.SaveChanges();

                        // Đăng nhập thành công với mật khẩu chưa mã hóa (sau khi cập nhật lại thành mã hóa)
                        SetUserSession(existingUser);
                        return RedirectBasedOnRole(existingUser.role);
                    }
                    else
                    {
                        // Sai mật khẩu
                        TempData["error"] = "Tài khoản đăng nhập không đúng.";
                        return View();
                    }
                }
                else
                {
                    TempData["error"] = "Tài khoản đăng nhập không đúng.";
                    return View();
                }
            }
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
        public ActionResult Profile(Tbl_User updatedUser)
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

            // Nếu có thay đổi, cập nhật và lưu vào cơ sở dữ liệu
            if (isUpdated)
            {
                user.ModifiedOn = DateTime.Now;
                ctx.SaveChanges();
                TempData["success"] = "Cập nhật thông tin thành công.";
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
                    //ViewBag.Name = "Quy";
                    //ViewBag.Address = "abc";
                    //ViewBag.PhoneNumber = "123";
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
                    existingItem.Quantity++;
                }
                else
                {
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

            return Json(new { success = false });
        }


        public ActionResult AddToCart(int productId, string url)
        {
            if (Session["cart"] == null)
            {
                List<Models.Home.Item> cart = new List<Models.Home.Item>();
                var product = ctx.Tbl_Product.Find(productId);
                cart.Add(new Models.Home.Item()
                {
                    Product = product,
                    Quantity = 1
                });
                Session["cart"] = cart;
            }
            else
            {
                List<Models.Home.Item> cart = (List<Models.Home.Item>)Session["cart"];
                var product = ctx.Tbl_Product.Find(productId);
                var existingItem = cart.FirstOrDefault(item => item.Product.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += 1;
                }
                else
                {
                    cart.Add(new Models.Home.Item()
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
            var cart = (List<Models.Home.Item>)Session["cart"];
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

        public ActionResult FailureView()
        {
            return View();
        }

        public ActionResult SuccessView()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Home/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            //on successful payment, show success page to user.  
            return View("SuccessView");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var listSanPham = Session["cart"] as List<Models.Home.Item>;
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<PayPal.Api.Item>()
            };
            foreach (var item in listSanPham)
            {
                itemList.items.Add(new PayPal.Api.Item()
                {
                    name = item.Product.ProductName,
                    currency = "USD",
                    price = "1",
                    quantity = item.Quantity.ToString(),
                    sku = "su",
                });
            }
            //Adding Item Details like name, currency, price etc  
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = "1"
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = "3", // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

    }
}