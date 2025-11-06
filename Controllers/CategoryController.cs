using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using LT_WebThoiTrang.Models;


namespace LT_WebThoiTrang.Controllers
{
    public class CategoryController : Controller
    {
        WebThoiTrangEntities db = new WebThoiTrangEntities();
        // GET: Category
        public ActionResult CategoryPage(int? id)

        {
            // kiểm tra id là null, lấy tất cả danh mục 
            var category = id.HasValue ? db.Categories.Find(id.Value) : null;
            if(!id.HasValue || category == null)
            {
                // lấy tất cả các sản phẩm và sắp xếp theo ID từ lớn đến bé 
                var products = db.Products
                   .OrderByDescending(p => p.ProductID) // xắp sếp theo ID từ lớn đến bé 
                   .ToList();
                ViewBag.Category = "Tất cả sản phẩm ";
                return View(products);
            }
            // lấy tất cả danh mục từ database 
            var categories = db.Categories.ToList();
            ViewBag.Categories = categories;

            // lọc sản phẩm theo category ID được truyền vào và sắp xếp theo ID từ lớn đến bé
            var productByCategories = db.Products
                .Where(p => p.CategoryID == id.Value)
                .OrderByDescending(p => p.ProductID)// sắp xếp theo id từ lớn đến bé 
                .ToList();
            ViewBag.CategoryName = category.CategoryName;
            ViewBag.CategoryID=category.CategoryID; 
            return View(productByCategories); ;
          
        }
        public ActionResult GetAllProducts()
        {
            var products = db.Products  
                .OrderByDescending (p => p.ProductID)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    CategoryName = db.Categories
                    .Where(c =>c.CategoryID == p.CategoryID )
                    .Select(c => c.CategoryName)
                    .FirstOrDefault()
                }).ToList();    
            return View(products);
        }
        public ActionResult SortProductsName(string data)
        {
            var products = db.Products
                 .Where(p => p.ProductName.Contains(data))// chi lay du l;ieu co ten data
                 .Select(p => new
                 {
                     p.ProductID,
                     p.ProductName,
                     p.Price,
                     p.ImageURL,
                     p.CategoryID,
                 }).ToList();
            return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SortProductsPrice(int price)
        {
            var products = db.Products
                .Where(p => p.Price > price) //chỉ lấy sản phẩm có giá trên 1000
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    p.CategoryID,

                }
                ).ToList();
            return Json(new {success = true , data =products}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllProducts(int? Idcategory)
        {
            // lấy danh sách sản phẩm từ cơ sở dữ liệu , có kiểm tra điều kiện categoryID
            var products = db.Products
                .Where(p => !Idcategory.HasValue || p.CategoryID == Idcategory) // nếu có categoryID ,lọc theo ID nếu không lấy tất cả 
                .OrderByDescending(p => p.ProductID) // sắp xếp theo ID từ lớn đến nhỏ 
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    p.Description,
                    // truy vấn lấy danh mục từ bảng Categories
                    CategoryName =db.Categories
                    .Where(c => c.CategoryID == Idcategory)
                    .Select(c => c.CategoryName)
                    .FirstOrDefault()
                }).ToList();
            return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllProductsJson (string sortBy)
        {
            var products = db.Products.AsQueryable();
            // sắp xếp sản phẩm dựa trên giá trị của 'sortBy
            switch(sortBy)
            {
                case "Newest":
                    products = products.OrderByDescending(p => p.ProductID);// sắp xếp sản phẩm mới nhất 
                    break;
                case "Oldest":
                    products = products.OrderBy(p => p.ProductID); // sắp xếp sản phẩm cũ nhất 
                    break;
                case "PriceDesc":
                    products =products.OrderByDescending (p => p.Price); // sắp xếp giá từ cao đến thấp 
                    break;
                case "PriceAsc":
                    products = products.OrderBy (p => p.Price);// sắp xếp giá từ thấp lên cao 
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName); // sắp xếp mặc định theo tên sản phẩm 
                    break;

            }
            // lấy danh sách sản phẩm và trả về dưới dạng JSON 
            var productList = products.Select(p => new
            {
                p.ProductID,
                p.ProductName,
                p.Price,
                p.ImageURL,
                p.Description,
            }).ToList();
            return Json(productList, JsonRequestBehavior.AllowGet);
        }

        
    }
}