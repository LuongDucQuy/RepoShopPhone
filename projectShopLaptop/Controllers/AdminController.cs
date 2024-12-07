using Newtonsoft.Json;
using projectShopLaptop.DAL;
using projectShopLaptop.Models;
using projectShopLaptop.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using projectShopLaptop.Filters;
using projectShopLaptop.Mail;
namespace projectShopLaptop.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        dbClickShopEntities ctx = new dbClickShopEntities();
        // GET: Admin

        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        // Kiểm tra xem người dùng đã đăng nhập chưa và xử lý cache
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Nếu người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
            if (Session["user"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Home"); // Chuyển hướng đến trang đăng nhập
            }

            // Ngừng lưu bộ nhớ cache trên các trang nhạy cảm
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
        }

        public List<SelectListItem> GetCategory()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var cat = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetAllRecords();
            foreach (var item in cat)
            {
                list.Add(new SelectListItem { Value = item.CategoryId.ToString(), Text = item.CategoryName });
            }
            return list;
        }

        public List<SelectListItem> GetCustomer()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var cat = _unitOfWork.GetRepositoryInstance<Tbl_User>().GetAllRecords();
            foreach (var item in cat)
            {
                list.Add(new SelectListItem { Value = item.user_id.ToString(), Text = item.Name });
            }
            return list;
        }

        public ActionResult CustomerAdd()
        {
            ViewBag.CustomerList = GetCustomer();
            return View();
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

        [HttpPost]
        public ActionResult CustomerAdd(Tbl_User tbl)
        {
            // Kiểm tra xem người dùng đã tồn tại chưa
            var existingUser = _unitOfWork.GetRepositoryInstance<Tbl_User>()
                .GetAllRecords()
                .FirstOrDefault(u => u.UserName == tbl.UserName || u.EmailId == tbl.EmailId); // Giả sử `Username` là khóa duy nhất

            if (existingUser != null)
            {
                if (existingUser.UserName == tbl.UserName)
                {
                    ModelState.AddModelError("", "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.");
                }
                else if (existingUser.EmailId == tbl.EmailId)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng. Vui lòng sử dụng email khác.");
                }

                ViewBag.CustomerList = GetCustomer();
                return View(tbl); // Trả lại view với thông báo lỗi
            }

            if (tbl.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự.");
                ViewBag.CustomerList = GetCustomer();
                return View(tbl);
            }

            tbl.role = "User";
            tbl.CreatedOn = DateTime.Now;
            tbl.Password = HashPassword(tbl.Password);
            _unitOfWork.GetRepositoryInstance<Tbl_User>().Add(tbl);
            return RedirectToAction("customerAccount");
        }


        public ActionResult AdminAdd()
        {
            ViewBag.CustomerList = GetCustomer();
            return View();
        }

        [HttpPost]
        public ActionResult AdminAdd(Tbl_User tbl)
        {
            // Kiểm tra xem người dùng đã tồn tại chưa
            var existingUser = _unitOfWork.GetRepositoryInstance<Tbl_User>()
                .GetAllRecords()
                .FirstOrDefault(u => u.UserName == tbl.UserName || u.EmailId == tbl.EmailId); // Giả sử `Username` là khóa duy nhất

            if (existingUser != null)
            {
                if (existingUser.UserName == tbl.UserName)
                {
                    ModelState.AddModelError("", "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.");
                }
                else if (existingUser.EmailId == tbl.EmailId)
                {
                    ModelState.AddModelError("", "Email đã được sử dụng. Vui lòng sử dụng email khác.");
                }

                ViewBag.CustomerList = GetCustomer();
                return View(tbl); // Trả lại view với thông báo lỗi
            }

            if (tbl.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự.");
                ViewBag.CustomerList = GetCustomer();
                return View(tbl);
            }
            tbl.role = "Admin";
            tbl.CreatedOn = DateTime.Now;
            tbl.Password = HashPassword(tbl.Password);
            _unitOfWork.GetRepositoryInstance<Tbl_User>().Add(tbl);
            return RedirectToAction("adminAccount");
        }

        [HttpGet]
        public ActionResult adminAccount()
        {
            var users = ctx.Tbl_User.Where(u => u.role == "Admin").ToList();
            return View(users);
        }

        [HttpGet]
        public ActionResult customerAccount()
        {
            var customer = ctx.Tbl_User.ToList();

            return View(customer);
        }
        [HttpPost]
        public ActionResult customerAccount(int id)
        {

            return RedirectToAction("customerEdit", new { userId = id });
        }

        [Route("XoaTaiKhoan")]
        [HttpPost]
        public ActionResult DeleteAccCtm(int id)
        {
            try
            {
                var admin = ctx.Tbl_User.Where(e => e.user_id == id).FirstOrDefault();

                if (admin != null)
                {
                    ctx.Tbl_User.Remove(admin);
                    ctx.SaveChanges();
                }
                else TempData["Message"] = "no";

            }
            catch (Exception ex)
            {

                TempData["Message"] = ex.ToString();

            }
            return RedirectToAction("customerAccount");
        }


        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                // Tìm sản phẩm dựa trên id
                var product = ctx.Tbl_Product.FirstOrDefault(e => e.ProductId == id);

                if (product != null)
                {
                    // Xóa sản phẩm nếu tồn tại
                    ctx.Tbl_Product.Remove(product);
                    ctx.SaveChanges();
                    TempData["Message"] = "Xóa sản phẩm thành công.";
                }
                else
                {
                    TempData["Message"] = "Sản phẩm không tồn tại.";
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có ngoại lệ
                TempData["Message"] = "Đã xảy ra lỗi khi xóa sản phẩm: " + ex.Message;
            }

            // Quay lại trang danh sách sản phẩm sau khi xóa
            return RedirectToAction("Product");
        }


        public ActionResult Order()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Bill>().GetProduct());
        }


        [HttpPost]
        public ActionResult ApproveOrder(int billId)
        {
            // Tìm đơn hàng theo billId
            var bill = ctx.Tbl_Bill.FirstOrDefault(b => b.IDBill == billId);
            var user = ctx.Tbl_User.Find(bill.user_id);

            if (bill != null)
            {
                // Đổi trạng thái của đơn hàng thành "Đã duyệt" (giả sử 2 là mã cho "Đã duyệt")
                bill.maTrangThai = 2;
                ctx.SaveChanges();

                // Định dạng tiền tệ cho số tiền, sử dụng CultureInfo của Việt Nam (VND)
                var vndCulture = new System.Globalization.CultureInfo("vi-VN");
                string formattedAmount = bill.soTien.ToString("C0", vndCulture); // Định dạng với ký hiệu tiền tệ (VND)

                // Tạo bảng HTML hiển thị thông tin khách hàng và đơn hàng
                string customerTable = "<table border='1' cellpadding='5' cellspacing='0'>" +
                                       "<tr><th>Tên Khách Hàng</th><th>Địa Chỉ</th><th>Điện Thoại</th><th>Số Tiền</th></tr>" +
                                       "<tr>" +
                                       $"<td>{bill.HoTen}</td>" +
                                       $"<td>{bill.diaChi}</td>" +
                                       $"<td>{bill.dienThoai}</td>" +
                                       $"<td>{formattedAmount}</td>" +  // Hiển thị số tiền theo định dạng VND
                                       "</tr>" +
                                       "</table>";

                // Tạo nội dung email với bảng khách hàng
                string message = $"Đơn hàng của bạn đã được duyệt thành công. Dưới đây là thông tin đơn hàng:<br><br>{customerTable}";

                // Gửi email thông báo
                SendMailOrder(user, message);
            }
            else
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("Order"); // Thay thế bằng tên Action phù hợp để quay lại trang quản lý đơn hàng.
        }


        public async Task SendMailOrder(Tbl_User user , string message)
        {
            string code = Guid.NewGuid().ToString();

            // Tạo đường dẫn xác nhận
            var callbackUrl = Url.Action("ConfirmEmail", "Mail", new { userId = user.user_id, code = code }, protocol: Request.Url.Scheme);

            // Gửi email xác nhận
            SendMail.SendEmail(user.EmailId, "Thông báo đơn hàng", message, "");
        }

        [HttpPost]
        public ActionResult DeliverOrder(int billId)
        {
            // Tìm đơn hàng theo billId
            var bill = ctx.Tbl_Bill.FirstOrDefault(b => b.IDBill == billId);

            if (bill != null)
            {
                bill.maTrangThai = 3;
                bill.ngayGiao = DateTime.Now;
                // Đặt mã trạng thái thành "Đang giao"
                ctx.SaveChanges();

                TempData["success"] = "Đơn hàng đang được giao.";
            }
            else
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("Order"); // Hoặc trang hiển thị đơn hàng
        }

        [HttpPost]
        public ActionResult ConfirmPayment(int billId)
        {
            // Tìm đơn hàng theo billId
            var bill = ctx.Tbl_Bill.FirstOrDefault(b => b.IDBill == billId);

            if (bill != null)
            {
                var user = ctx.Tbl_User.Find(bill.user_id);

                if (user != null)
                {
                    // Đặt mã trạng thái thành "Đã giao"
                    bill.maTrangThai = 4;
                    ctx.SaveChanges();
                    SendMailOrder(user, "Đơn hàng của bạn đã được xác nhận thanh toán.");
                }
                else
                {
                    TempData["error"] = "Không tìm thấy thông tin người dùng.";
                }
            }
            else
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("Order"); // Hoặc trang hiển thị đơn hàng
        }

        [HttpPost]
        public ActionResult CancelOrder(int billId)
        {
            var bill = ctx.Tbl_Bill.FirstOrDefault(b => b.IDBill == billId);
            if (bill != null)
            {
                bill.maTrangThai = 5; // Giả sử 5 là mã trạng thái cho "Đã hủy"
                ctx.SaveChanges();
            }
            return RedirectToAction("Order"); // Quay lại trang danh sách đơn hàng sau khi hủy đơn
        }

        public ActionResult purchaseInformation()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Categories()
        {
            List<Tbl_Category> allcategories = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetAllRecordsIQueryable().Where(i => i.IsDelete == false).ToList();
            return View(allcategories);
        }

        [HttpPost]
        public async Task<ActionResult> AddCategory(string nameCate)
        {
            if (string.IsNullOrEmpty(nameCate))
            {
                return Json(new { success = false, message = "Thông tin đầu vào không hợp lệ." });
            }

            var cateNew = new Tbl_Category
            {
                CategoryName = nameCate,
                IsDelete = false,
                IsActive = true
            };

            try
            {
                ctx.Tbl_Category.Add(cateNew);
                await ctx.SaveChangesAsync();

                var updatedCategories = await ctx.Tbl_Category.Where(i => i.IsDelete == false)
                                .Select(c => new
                                {
                                    c.CategoryId,
                                    c.CategoryName,
                                    // Chỉ chọn các thuộc tính cần thiết để tránh vòng lặp
                                })
                                .ToListAsync();
                return Json(new { success = true, categories = updatedCategories });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (nếu cần)
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm danh mục.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCate(int id, string nameCate)
        {
            Console.WriteLine("ok");
            if (string.IsNullOrEmpty(nameCate))
            {
                return Json(new { success = false, message = "Thông tin đầu vào không hợp lệ." });
            }

            var category = await ctx.Tbl_Category.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại." });
            }

            category.CategoryName = nameCate;

            try
            {
                await ctx.SaveChangesAsync();
                // Lấy danh sách danh mục cập nhật
                var updatedCategories = await ctx.Tbl_Category.Where(i => i.IsDelete == false)
                                .Select(c => new
                                {
                                    c.CategoryId,
                                    c.CategoryName,
                                    // Chỉ chọn các thuộc tính cần thiết để tránh vòng lặp
                                })
                                .ToListAsync();
                return Json(new { success = true, categories = updatedCategories });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (nếu cần)
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật danh mục.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProduct(int id, string nameCate)
        {
            Console.WriteLine("ok");
            if (string.IsNullOrEmpty(nameCate))
            {
                return Json(new { success = false, message = "Thông tin đầu vào không hợp lệ." });
            }

            var category = await ctx.Tbl_Product.FirstOrDefaultAsync(c => c.ProductId == id);
            if (category == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại." });
            }

            category.ProductName = nameCate;

            try
            {
                await ctx.SaveChangesAsync();
                // Lấy danh sách danh mục cập nhật
                var updatedCategories = await ctx.Tbl_Product
                                .Select(c => new
                                {
                                    c.ProductId,
                                    c.ProductName,
                                    // Chỉ chọn các thuộc tính cần thiết để tránh vòng lặp
                                })
                                .ToListAsync();
                return Json(new { success = true, categories = updatedCategories });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (nếu cần)
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật danh mục.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddProduct(string nameCate)
        {
            if (string.IsNullOrEmpty(nameCate))
            {
                return Json(new { success = false, message = "Thông tin đầu vào không hợp lệ." });
            }

            var cateNew = new Tbl_Product
            {
                ProductName = nameCate,
                IsDelete = false,
                IsActive = true
            };

            try
            {
                ctx.Tbl_Product.Add(cateNew);
                await ctx.SaveChangesAsync();

                var updatedCategories = await ctx.Tbl_Product.Where(i => i.IsDelete == false)
                                .Select(c => new
                                {
                                    c.ProductId,
                                    c.ProductName,
                                    // Chỉ chọn các thuộc tính cần thiết để tránh vòng lặp
                                })
                                .ToListAsync();
                return Json(new { success = true, categories = updatedCategories });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (nếu cần)
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm danh mục.", error = ex.Message });
            }
        }

        public ActionResult Customer()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_User>().GetProduct());
        }

        [HttpGet]
        public ActionResult customerEdit(int id)
        {
            Tbl_User user = ctx.Tbl_User.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }


        [HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult customerEdit(int user_id, string Name, string UserName, string EmailId, string Password)
        {
            TempData["Message"] = "";

            if (ModelState.IsValid)
            {
                if (user_id <= 0)
                {
                    // Xử lý trường hợp user_id không hợp lệ
                    ModelState.AddModelError("user_id", "ID người dùng không hợp lệ.");
                    return View();
                }

                // Truy vấn người dùng theo user_id
                Tbl_User existingUser = ctx.Tbl_User.Find(user_id);

                // Không rỗng và không phải là null
                if (existingUser != null)
                {
                    // Cập nhật thông tin người dùng
                    existingUser.Name = Name;
                    existingUser.UserName = UserName;
                    existingUser.EmailId = EmailId;
                    existingUser.Password = HashPassword(Password);
                    existingUser.CreatedOn = DateTime.Now;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    ctx.SaveChanges();
                    TempData["Message"] = "Cập nhật tài khoản thành công";

                    // Chuyển hướng đến một view hoặc action thành công
                    return RedirectToAction("customerAccount");
                }

                // Xử lý trường hợp không tìm thấy người dùng với user_id đã cho
                ModelState.AddModelError("", "Không tìm thấy người dùng.");
            }

            // Nếu kiểm tra model thất bại hoặc có bất kỳ lỗi nào khác, trở về view chỉnh sửa
            return View();
        }


        public ActionResult Product()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Product>().GetProduct());
        }

        public ActionResult ProductEdit(int productId)
        {
            ViewBag.CategoryList = GetCategory();
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Product>().GetFirstorDefault(productId));
        }

        [HttpPost]
        public ActionResult ProductEdit(Tbl_Product tbl, HttpPostedFileBase file)
        {
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/img/"), pic);
                file.SaveAs(path);
            }
            tbl.ProductImage = file != null ? pic : tbl.ProductImage;
            tbl.ModifiedDate = DateTime.Now;
            _unitOfWork.GetRepositoryInstance<Tbl_Product>().Update(tbl);
            return RedirectToAction("Product");
        }
        public ActionResult ProductAdd()
        {
            ViewBag.CategoryList = GetCategory();
            return View();
        }

        [HttpPost]
        public ActionResult ProductAdd(Tbl_Product tbl, HttpPostedFileBase file)
        {
            string pic = null;
            if (file != null)
            {
                pic = System.IO.Path.GetFileName(file.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/img/"), pic);
                file.SaveAs(path);
            }
            tbl.ProductImage = pic;
            tbl.CreateDate = DateTime.Now;
            _unitOfWork.GetRepositoryInstance<Tbl_Product>().Add(tbl);
            return RedirectToAction("Product");
        }
    }
}