using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LT_WebThoiTrang.Models;
using System.Net;

namespace LT_WebThoiTrang.Controllers
{
    public class ProductPageController : Controller
    {
        // GET: ProductPage
        private WebThoiTrangEntities db= new WebThoiTrangEntities();// Sử dụng db context đã được tạo từ entity FrameWork
        public ActionResult ProductPage(int? id)
        {
            if(id ==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.Products.Find(id); // db là context của Entity Framework
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product); // truyền sản phẩm vào View

        }
        [HttpGet]
        public JsonResult GetProduct(int id)
        {
            var product = db.Products
                .Where(p => p.ProductID == id)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.Description,
                    p.ImageURL,
                    p.CategoryID,
                    // Kiểm tra nếu sản phẩm có khuyến mãi đang hoạt động 
                    IdDiscounted = db.ProductPromotions
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
                    .Select(pr => pr.DiscountAmount).FirstOrDefault()).FirstOrDefault(),

                    DiscountPercentage = db.ProductPromotions
                    .Where(pp => pp.ProductID == p.ProductID)
                    .Select(pp => db.Promotions
                    .Where( pr => pr.PromotionID == pp.PromotionID &&
                            pr.StartDate <= DateTime.Now &&
                            pr.EndDate >= DateTime.Now)
                    .Select(pr => pr.DiscountPercentage).FirstOrDefault()).FirstOrDefault(),  

                    // Lấy danh sách ảnh phụ 
                    SupplementaryImage = db.ImageProducts
                    .Where(img => img.ProductsID == p.ProductID)
                    .Select (img => img.ImageURL)
                    .ToList()
                    
                })
                .FirstOrDefault();

            if(product == null )
            {
                return Json(new {success = false , message = "Product not found "}, JsonRequestBehavior.AllowGet);
            }    
            return Json(new {success = true , data = product}, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProductSizeWithOutQuantity( int productID )
        {
            // kiểm trra xem sản phẩm có tồn tại hay không 
            var product = db.Products.Find(productID);
            if(product ==null)
            {
                return Json(new { success = false, message = "Product not exits" }, JsonRequestBehavior.AllowGet);
            }
            // lấy danh sách kích thước của sản phẩm 
            var sizeProduct = db.ProductStocks
                .Where(ps => ps.ProductID == productID)
                .Select(ps => db.Sizes
                        .Where(s => s.SizeID == ps.SizeID)
                        .Select(s =>s.Size1)
                        .FirstOrDefault())
            .Distinct()
            .ToList();
            //Trả về danh sách kích thước và tên sản phẩm dưới dạng JSON
            return Json(new
            {
                success = true,
                productName = product.ProductName, // bao gồm tên sản phẩm 
                sizes = sizeProduct // danh sách kích thước
            },JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetRelatedProducts(int productId)
        {
            // lấy sản phẩm hiện tại kiểm tra CategoryID
            var currentProduct = db.Products.Find(productId);
            // kiểm tra sản phẩm hiện tại 
            if(currentProduct == null)
            {
                return Json(new {success =false , message= "Product can't find "},JsonRequestBehavior.AllowGet);
            }
            // lấy các sản phẩm có cùng category ID, ngoại trừ sản phẩm hiện tại 
            var relatedProducts = db.Products
                .Where(p => p.CategoryID == currentProduct.CategoryID &&
                p.ProductID != productId)
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.ImageURL,
                    p.Description
                }).ToList();
            return Json(new {success = true , date =relatedProducts},JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProductImages(int productId)
        {
            // kiểm tra sản phẩm có tồn tại hay không 
            var product =db.Products.Find(productId);
            if(product == null)
            {
                return Json(new { success = false, message = "Sản phẩm khoogn tìm thấy " }, JsonRequestBehavior.AllowGet);
            }    
            // lấy danh sách ảnh phụ của sản phẩm 
            var supplementaryImages = db.ImageProducts
                        .Where(img => img.ProductsID == productId)
                        .Select(img => img.ImageURL)
                        .ToList();
            if (supplementaryImages == null || supplementaryImages.Count == 0)
            {
                return Json(new { success = false, message = " không có ảnh phụ cho sản phẩm này " }, JsonRequestBehavior.AllowGet);
            }
            // trả về danh sách dưới dạng JSon
            return Json(new
            {
                success = true,
                productName = product.ProductName,// bao gồm tên sản phẩm 
                images = supplementaryImages// danh sách ảnh phụ
            },JsonRequestBehavior.AllowGet);
        }
    }
}