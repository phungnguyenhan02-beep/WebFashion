using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; }  // Danh sách các sản phẩm trong giỏ hàng

        public Cart()
        {
            Items = new List<CartItem>();  // Khởi tạo danh sách giỏ hàng trống
        }

        // Thêm một sản phẩm vào giỏ hàng
        public void AddItem(CartItem item)
        {
            // Kiểm tra nếu sản phẩm đã có trong giỏ hàng (tính theo ProductID và Size)
            var existingItem = Items.FirstOrDefault(i => i.ProductID == item.ProductID && i.Size == item.Size);
            if (existingItem != null)
            {
                // Nếu sản phẩm đã có, tăng số lượng
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới vào giỏ hàng
                Items.Add(item);
            }
        }


        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public void UpdateItem(int productId, string size, int quantity)
        {
            // Tìm sản phẩm trong giỏ theo ProductID và Size
            var item = Items.FirstOrDefault(i => i.ProductID == productId && i.Size == size);
            if (item != null)
            {
                item.Quantity = quantity;  // Cập nhật số lượng của sản phẩm
            }
        }


        // Xóa sản phẩm khỏi giỏ hàng
        public void RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                Items.Remove(item);  // Xóa sản phẩm khỏi giỏ hàng
            }
        }

        // Tính tổng số lượng sản phẩm trong giỏ hàng
        // Tính tổng số lượng sản phẩm trong giỏ hàng
        public int GetTotalQuantity()
        {
            return Items.Sum(i => i.Quantity);
        }

        // Tính tổng tiền của giỏ hàng
        public decimal Total
        {
            get { return Items.Sum(i => i.TotalPrice); }
        }
    }
}
    