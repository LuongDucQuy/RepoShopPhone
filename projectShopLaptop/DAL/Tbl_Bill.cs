//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace projectShopLaptop.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tbl_Bill
    {
        public int IDBill { get; set; }
        public int user_id { get; set; }
        public System.DateTime ngayDat { get; set; }
        public Nullable<System.DateTime> ngayCan { get; set; }
        public Nullable<System.DateTime> ngayGiao { get; set; }
        public string HoTen { get; set; }
        public string diaChi { get; set; }
        public string dienThoai { get; set; }
        public string cachThanhToan { get; set; }
        public string cachVanChuyen { get; set; }
        public Nullable<int> maTrangThai { get; set; }
        public string ghiChu { get; set; }
        public decimal soTien { get; set; }
        public Nullable<int> ProductId { get; set; }
    
        public virtual Tbl_User Tbl_User { get; set; }
        public virtual Tbl_Product Tbl_Product { get; set; }
    }
}
