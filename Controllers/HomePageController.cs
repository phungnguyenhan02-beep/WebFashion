using LT_WebThoiTrang.Models;
using LT_WebThoiTrang.Services;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LT_WebThoiTrang.Controllers
{
    public class HomePageController : Controller
    {
        private WebThoiTrangEntities db = new WebThoiTrangEntities(); // Sử dụng DbContext đã được tạo từ Entity Framework
        // GET: HomePage
        public ActionResult Home_page()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetNoStore();
            ViewBag.IsAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
            return View();
        }
        [HttpGet]
        public ActionResult GetAllProducts()
        {
            var products = db.Products
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    CategoryName = db.Categories
                        .Where(c => c.CategoryID == p.CategoryID)
                        .Select(c => c.CategoryName)
                        .FirstOrDefault()
                })
                .ToList();

            return View(products); // Trả về View với model là danh sách sản phẩm
        }

        [HttpGet]
        public JsonResult GetAllProductsJson(int? idCategory)
        {
            // Lấy danh sách sản phẩm từ cơ sở dữ liệu, có kiểm tra điều kiện idCategory
            var products = db.Products
               .Where(p => !idCategory.HasValue || p.CategoryID == idCategory)
                    .OrderByDescending(p => p.ProductID) // Sắp xếp theo ID sản phẩm mới nhất
                    .Select(p => new
                    {
                        p.ProductID,
                        p.ProductName,
                        p.Price,
                        p.ImageURL,
                        p.Description,
                        CategoryName = db.Categories
                            .Where(c => c.CategoryID == p.CategoryID)
                            .Select(c => c.CategoryName)
                            .FirstOrDefault()
                    })
                    .ToList();

            // Trả về dữ liệu JSON
            return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllProductsPromotion()
        {
            /* ghi chú dành riêng cho FrontEnd
                  {
                  "success": true,
                  "data": [
                    {
                      "ProductID": 1,
                      "ProductName": "Product A",
                      "Price": 100.00,
                      "ImageURL": "/images/productA.jpg",
                      "Description": "Description",
                      "IsDiscounted": true,
                      "DiscountAmount": 10.00,
                      "DiscountPercentage": null
                    },
                    {
                      "ProductID": 2,
                      "ProductName": "Product B",
                      "Price": 150.00,
                      "ImageURL": "/images/productB.jpg",
                      "Description": "Description",
                      "IsDiscounted": false,
                      "DiscountAmount": null,
                      "DiscountPercentage": null
                    },
                    {
                      "ProductID": 3,
                      "ProductName": "Product C",
                      "Price": 200.00,
                      "ImageURL": "/images/productC.jpg",
                      "Description": "Description",
                      "IsDiscounted": true,
                      "DiscountAmount": null,
                      "DiscountPercentage": 15.0
                    }
                  ]
                }

*/
            var products = db.Products
             .Select(p => new
             {
                 p.ProductID,
                 p.ProductName,
                 p.Price,
                 p.ImageURL,
                 p.Description,
                 // Kiểm tra nếu sản phẩm có khuyến mãi đang hoạt động
                 IsDiscounted = db.ProductPromotions
                     .Any(pp => pp.ProductID == p.ProductID &&
                                db.Promotions.Any(pr => pr.PromotionID == pp.PromotionID &&
                                                        pr.StartDate <= DateTime.Now &&
                                                        pr.EndDate >= DateTime.Now)),
                 // Lấy chi tiết khuyến mãi nếu có
                 DiscountAmount = db.ProductPromotions
                     .Where(pp => pp.ProductID == p.ProductID)
                     .Select(pp => db.Promotions
                         .Where(pr => pr.PromotionID == pp.PromotionID &&
                                      pr.StartDate <= DateTime.Now &&
                                      pr.EndDate >= DateTime.Now)
                         .Select(pr => pr.DiscountAmount)
                         .FirstOrDefault())
                     .FirstOrDefault(),
                 DiscountPercentage = db.ProductPromotions
                     .Where(pp => pp.ProductID == p.ProductID)
                     .Select(pp => db.Promotions
                         .Where(pr => pr.PromotionID == pp.PromotionID &&
                                      pr.StartDate <= DateTime.Now &&
                                      pr.EndDate >= DateTime.Now)
                         .Select(pr => pr.DiscountPercentage)
                         .FirstOrDefault())
                     .FirstOrDefault()
             })
             .ToList();
            return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAllLevent()
        {
            // Lọc tất cả các sản phẩm có CategoryID là 4 (áo đấu)
            var products = db.Products
                 .Where(p => p.Category.CategoryID == 4)
                .OrderByDescending(p => p.ProductID) // Sắp xếp theo ID sản phẩm mới nhất
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    p.Description,
                    // Truy vấn lấy tên danh mục từ bảng Categories
                    categoryID = p.Category.CategoryID
                })
                .ToList();

            // Trả về dữ liệu JSON
            return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
        }
    }
}
