using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace projectShopLaptop.Models.Home
{
    public class Register
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage ="*")]
        [MaxLength(50, ErrorMessage ="Tối đa 20 kí tự")]
        public string FirstName { get; set; }

        [Display(Name ="Nhập tên")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 kí tự")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Chưa đúng định dạng email")]
        public string EmailId { get; set; }

        [Display(Name ="Mật khẩu")]
        [Required(ErrorMessage = "*")]
        public string Password { get; set; }
    }
}