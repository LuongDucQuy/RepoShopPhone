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

namespace projectShopLaptop.Controllers
{
    public class AdminController : Controller
    {
        dbClickShopEntities ctx = new dbClickShopEntities();
        // GET: Admin

        public GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
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

        public ActionResult Dashboard()
        {
            return View();
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
                .FirstOrDefault(u => u.UserName == tbl.UserName); // Giả sử `Username` là khóa duy nhất

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.");
                ViewBag.CustomerList = GetCustomer();
                return View(tbl); // Trả lại view với thông báo lỗi
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
            tbl.role = "Admin";
            tbl.CreatedOn = DateTime.Now;
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
            var users = ctx.Tbl_User.Where(u => u.role == "User").ToList();
            return View(users);
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

            if (bill != null)
            {
                // Đổi trạng thái của đơn hàng thành "Đã duyệt" (giả sử 2 là mã cho "Đã duyệt")
                bill.maTrangThai = 2;
                ctx.SaveChanges();

                TempData["success"] = "Đơn hàng đã được duyệt thành công.";
            }
            else
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("Order"); // Thay thế bằng tên Action phù hợp để quay lại trang quản lý đơn hàng.
        }

        [HttpPost]
        public ActionResult DeliverOrder(int billId)
        {
            // Tìm đơn hàng theo billId
            var bill = ctx.Tbl_Bill.FirstOrDefault(b => b.IDBill == billId);

            if (bill != null)
            {
                bill.maTrangThai = 3; // Đặt mã trạng thái thành "Đang giao"
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
                bill.maTrangThai = 4; // Đặt mã trạng thái thành "Đã giao"
                ctx.SaveChanges();

                TempData["success"] = "Đơn hàng đã được xác nhận thanh toán và chuyển trạng thái thành 'Đã giao'.";
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

        public ActionResult Categories()
        {
            List<Tbl_Category> allcategories = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetAllRecordsIQueryable().Where(i => i.IsDelete == false).ToList();
            return View(allcategories);
        }

        public ActionResult AddCategory()
        {
            return UpdateCategory(0);
        }

        public ActionResult UpdateCategory(int? categoryId)
        {
            CategoryDetail cd = new CategoryDetail();
            if (categoryId.HasValue && categoryId > 0)
            {
                cd = JsonConvert.DeserializeObject<CategoryDetail>(
                    JsonConvert.SerializeObject(_unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(categoryId.Value))
                );
            }
            return View("UpdateCategory", cd);
        }

        [HttpPost]
        public ActionResult UpdateCategory(CategoryDetail model)
        {
            if (ModelState.IsValid)
            {
                if (model.CategoryId > 0)
                {
                    var category = _unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(model.CategoryId);
                    if (category != null)
                    {
                        category.CategoryName = model.CategoryName;
                        _unitOfWork.GetRepositoryInstance<Tbl_Category>().Update(category);
                    }
                }
                else
                {
                    var newCategory = new Tbl_Category
                    {
                        CategoryName = model.CategoryName,
                        IsActive = true,
                        IsDelete = false 
                    };
                    _unitOfWork.GetRepositoryInstance<Tbl_Category>().Add(newCategory);
                }

                return RedirectToAction("Categories");
            }

            return View(model);
        }


        public ActionResult CategoryEdit(int catId)
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_Category>().GetFirstorDefault(catId));
        }

        [HttpPost]
        public ActionResult CategoryEdit(Tbl_Category tbl)
        {
            _unitOfWork.GetRepositoryInstance<Tbl_Category>().Update(tbl);
            return RedirectToAction("Categories");
        }

        public ActionResult Customer()
        {
            return View(_unitOfWork.GetRepositoryInstance<Tbl_User>().GetProduct());
        }

        public ActionResult customerEdit(int customerId)
        {
            ViewBag.CustomerList = GetCustomer();
            return View(_unitOfWork.GetRepositoryInstance<Tbl_User>().GetFirstorDefault(customerId));
        }

        [HttpPost]
        public ActionResult customerEdit(Tbl_User tbl)
        {
            _unitOfWork.GetRepositoryInstance<Tbl_User>().Update(tbl);
            return RedirectToAction("customerAccount");
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