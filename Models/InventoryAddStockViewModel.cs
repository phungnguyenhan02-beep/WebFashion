using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class InventoryAddStockViewModel
    {
        public int ProductStockID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string MainImageUrl { get; set; }
        public string Size { get; set; }
        public int CurrentStockQuantity { get; set; } // Số lượng hiện tại trong kho
        public int AddStockQuantity { get; set; } // Số lượng muốn thêm vào kho
        public int StockQuantity { get; set; } // Thuộc tính StockQuantity phải có ở đây

        public int? SupplierID { get; set; } // ID nhà cung cấp
        public int? UserID { get; set; } // ID người nhập kho
        public string Color { get; internal set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải là số dương")]
        public int QuantityAdded { get; set; }
        public int SizeID { get; set; }
    }
}