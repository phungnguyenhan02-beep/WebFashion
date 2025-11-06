using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LT_WebThoiTrang.Models;

namespace LT_WebThoiTrang.Models
{
    public class ProductViewModelcs
    { 
        public Product Product { get; set; }
        public string CategoryName { get; set; }
        public List<ProductStockViewModel> ProductStocks { get; set; } // Sử dụng ProductStockViewModel để chứa SizeValue
        public List<int> SelectedSizeIDs { get; set; } = new List<int>();
        public List<HttpPostedFileBase> AdditionalImages { get; set; }
        public List<ImageProduct> ExistingImages { get; set; }
        public List<string> AdditionalImagesUrls { get; set; } = new List<string>();
        public string MainImageUrl { get; set; }
        public int SelectedColorID { get; set; }
        public int SelectedSizeID { get; set; }
        public int Quantity { get; set; }
        public IEnumerable<Color> Colors { get; set; }
        public IEnumerable<Size> Sizes { get; set; }
    }

    // Tạo ProductStockViewModel để chứa SizeValue
    public class ProductStockViewModel
    {
        public int ProductStockID { get; set; }
        public int ProductID { get; set; }
        public int ColorID { get; set; }
        public int sizeID { get; set; }
        public string SizeValue { get; set; } // Đảm bảo có thuộc tính này
        public int Quantity { get; set; }
    }
}
