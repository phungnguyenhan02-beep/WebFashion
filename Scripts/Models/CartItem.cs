using System;

namespace LT_WebThoiTrang.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }  // ID sản phẩm
        public string ProductName { get; set; }  // Tên sản phẩm
        public int Price { get; set; }  // Giá sản phẩm
        public int Quantity { get; set; }  // Số lượng sản phẩm
        public string Size { get; set; }
        public string ImageUrl { get; set; }  // URL hình ảnh sản phẩm

        // Tính tổng tiền cho một sản phẩm (Quantity * Price)
        public decimal TotalPrice
        {
            get { return Quantity * Price; }
        }
        public DateTime DateAdded { get; set; }  // Thời gian thêm vào giỏ hàng
    }
}
