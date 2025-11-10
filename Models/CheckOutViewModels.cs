using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class CheckOutViewModels
    {
        public AddressUser Address { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }
}