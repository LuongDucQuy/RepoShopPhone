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
    
    public partial class Tbl_Category
    {
        public Tbl_Category()
        {
            this.Tbl_Product = new HashSet<Tbl_Product>();
        }
    
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDelete { get; set; }
        public string CategoryImage { get; set; }
    
        public virtual ICollection<Tbl_Product> Tbl_Product { get; set; }
    }
}
