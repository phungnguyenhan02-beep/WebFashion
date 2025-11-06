using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.Management;
using System.Web.Mvc;
using System.Xml.Linq;
using LT_WebThoiTrang.Models;
using LT_WebThoiTrang.Services;


namespace LT_WebThoiTrang.Controllers
{
    public class AdminController : Controller
    {
        private WebThoiTrangEntities db = new WebThoiTrangEntities();



        // GET: Admin
        public ActionResult DashBoard()
        {
            // lấy số dư tháng hiện tại và truyền vào view bag
            ViewBag.CurrentMonthBalance = GetCurrentMonthBalance();
            // lấy số dư năm hiện tại  và truyền vào view bag 
            ViewBag.CurrentYearBalance = GetCurrentYearBalance();
            // Lấy số lượng đơn hàng hiện tại
            ViewBag.CurrentMonthOrderCount = GetCurrentMonthOrderCount();
            // tính tổng số người dùng 
            ViewBag.CurrentTotalUsersCount = GetTotalUserCount();


            return View();
        }
        public int GetCurrentMonthBalance()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);//ngày đầu tháng 
            var endDate = startDate.AddMonths(1); //ngày đầu tháng sau
            // tính tổng thu nhập của tháng hiện tại từ bảng Orders
            var currentMonthBalance = db.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .Sum(o => (int?)o.price) ?? 0; // tính tổng thu nhập từ cột 'price'
            return currentMonthBalance;

        }
        public int GetCurrentYearBalance()
        {
            var startYear = new DateTime(DateTime.Now.Year, 1, 1);//ngày đầu năm
            var startNextYear = new DateTime(DateTime.Now.Year + 1, 1, 1); //ngày bắt đầu năm sau 
            // tính tổng thu nhập của tháng hiện tại từ bảng Orders
            var currentYearBalance = db.Orders
                .Where(o => o.OrderDate >= startYear && o.OrderDate < startNextYear)
                .Sum(o => (int?)o.price) ?? 0; // tính tổng thu nhập từ cột 'price'
            return currentYearBalance;

        }
        public int GetCurrentMonthOrderCount()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1);
            // Đếm số lượng đợn hàng trong khoảng thời gian từ ngày đầu tiên cuối tháng 
            int orderCount = db.Orders?
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .Count() ?? 0;
            return orderCount;
        }
        public int GetTotalUserCount()
        {
            // đếm tổng số người dùng trong bảng 
            int userCount = db.Users.Count();
            return userCount;
        }


        //Get:Admin/MangeProducts
        public ActionResult MangeProducts()
        {
            // lấy tất cả  sản phẩm 
            var products = db.Products.ToList();

            // Tạo danh sách ProductViewModel cho mỗi sản phẩm 
            var productViewModels = new List<ProductViewModelcs>();

            foreach (var product in products)
            {
                var productViewModel = new ProductViewModelcs
                {
                    Product = product,
                    // lấy tên danh mục 
                    CategoryName = db.Categories
                    .Where(c => c.CategoryID == product.CategoryID)
                    .Select(c => c.CategoryName)
                    .FirstOrDefault(),
                    // lấy danh sách Productstock và  bao gồm size value 
                    ProductStocks = db.ProductStocks
                    .Where(ps => ps.ProductID == product.ProductID)
                    .Select(ps => new ProductStockViewModel
                    {
                        ProductStockID = ps.ProductStockID,
                        ProductID = (int)ps.ProductID,
                        ColorID = (int)ps.ColorID,

                        // lấy size value  từ bảng sizes dựa trên size Id
                        SizeValue = db.Sizes
                                .Where(s => s.SizeID == ps.SizeID)
                                .Select(s => s.Size1)
                                .FirstOrDefault(),
                        Quantity = (int)ps.Quantity
                    }).ToList(),
                    // Lấy danh sách ảnh phụ
                    ExistingImages = db.ImageProducts
                        .Where(img => img.ProductsID == product.ProductID)
                        .ToList()
                };
                productViewModels.Add(productViewModel);
            }
            // Tra ve danh sach ProductViewModel vao view 
            return View(productViewModels);
        }

        //GET Admin/ Mange Category
        public ActionResult MangeCategory()
        {
            var categories = db.Categories.ToList();// lấy danh sách tất cả các loại sản phẩm 
            return View(categories);
        }

        // Admin/CreateProduct
        public ActionResult CreateProducts()
        {
            var model = new ProductViewModelcs
            {
                Colors = db.Colors.ToList(),
                Sizes = db.Sizes.ToList(),
            };
            ViewBag.Categories = new SelectList(db.Categories, "CategoryId", "CategoryName");
            return View(model);
        }
        [HttpPost]
        public ActionResult CreateProducts(ProductViewModelcs model, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // khai báo allowedExtensions cho các định dạng tệp tin hợp lệ 
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                // xử lý ảnh chính 
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string extension = Path.GetExtension(ImageFile.FileName).ToLower();
                    if (allowedExtensions.Contains(extension))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + extension;
                        string path = Path.Combine(Server.MapPath("~/image/prodcut-image/"), fileName);
                        ImageFile.SaveAs(path);
                        model.Product.ImageURL = fileName; ;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid file type. Please upload a JPG, JPEG, PNG, or GIF image.";
                        model.Colors = db.Colors.ToList();
                        model.Sizes = db.Sizes.ToList();
                        return View(model);
                    }
                }
                // Thêm sản phẩm vào bảng Products 
                db.Products.Add(model.Product);
                db.SaveChanges();

                //Thêm thông tin sản phẩm vào ProductStocks cho từng kích thước được chọn 
                if (model.SelectedSizeIDs != null && model.SelectedSizeIDs.Count > 0)
                {
                    foreach (var sizeId in model.SelectedSizeIDs)
                    {
                        var productStock = new ProductStock
                        {
                            ProductID = model.Product.ProductID,
                            ColorID = model.SelectedColorID,
                            SizeID = sizeId,
                            Quantity = model.Quantity
                        };
                        db.ProductStocks.Add(productStock);
                    }
                    db.SaveChanges();
                }
                // xử lý và lưu ảnh phụ 
                if (model.AdditionalImages != null && model.AdditionalImages.Count > 0)
                {
                    foreach (var additionalImage in model.AdditionalImages)
                    {
                        if (additionalImage != null && additionalImage.ContentLength > 0)
                        {
                            string extension = Path.GetExtension(additionalImage.FileName).ToLower();
                            if (allowedExtensions.Contains(extension))
                            {
                                // Tạo tên file duy nhất cho mỗi ảnh phụ
                                string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + "_sub" + extension;
                                string path = Path.Combine(Server.MapPath("~/image/product-image/"), fileName);
                                additionalImage.SaveAs(path);

                                var imageProduct = new ImageProduct
                                {
                                    ProductsID = model.Product.ProductID,
                                    ImageURL = fileName
                                };
                                db.ImageProducts.Add(imageProduct);
                            }
                        }
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("ManageProducts");
            }
            // Nếu ModelState không hợp lệ, tải lại danh sách Colors và Sizes
            model.Colors = db.Colors.ToList();
            model.Sizes = db.Sizes.ToList();
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(model);
        }
        [HttpGet]
        public ActionResult EditProducts(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // lấy sản phẩm từ cơ sở dữ liệu 
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            // lấy danh sách ảnh từ phụ từ ImageProducts.
            var additionalImageUrls = db.ImageProducts
                .Where(img => img.ProductsID == product.ProductID)
                .Select(img => img.ImageURL)
                .ToList();
            //tạo view model và truyền dữ liệu vào viewModel
            var model = new ProductViewModelcs
            {
                Product = product,
                SelectedColorID = db.ProductStocks.FirstOrDefault(ps => ps.ProductID == product.ProductID)?.ColorID ?? 0,
                Colors = db.Colors.ToList(),
                AdditionalImagesUrls = additionalImageUrls  // gán danh sách URL ảnh phụ 
            };
            //truyền danh sách vào Categories cho ViewBag để hiện thị trong dropdown
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProducts(ProductViewModelcs model, HttpPostedFileBase ImageFile, IEnumerable<HttpPostedFileBase> AdditionalImages)
        {
            if (ModelState.IsValid)
            {
                // lấy sản phẩm từ cơ sở dữ liệu 
                var product = db.Products.Find(model.Product.ProductID);
                if (product != null)
                {
                    // cập nhật thông tin sản phẩm 
                    product.ProductName = model.Product.ProductName;
                    product.Description = model.Product.Description;
                    product.Price = model.Product.Price;
                    product.CategoryID = model.Product.CategoryID;

                    // cập nhật màu sắc nếu cần 
                    var productStock = db.ProductStocks.FirstOrDefault(ps => ps.ProductID == model.Product.ProductID);
                    if (productStock != null)
                    {
                        productStock.ColorID = model.SelectedColorID;
                        db.Entry(productStock).State = EntityState.Modified;
                    }
                    // xử lý hình ảnh chính nếu có file mới 
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(ImageFile.FileName).ToLower();
                        string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
                        string path = Path.Combine(Server.MapPath("~/Image/product-image/"), fileName);
                        ImageFile.SaveAs(path);
                        product.ImageURL = fileName;// lưu tên file vào cơ sở dữ liệu 

                    }
                    // xử lý ảnh phụ nếu có file mới 
                    if (AdditionalImages != null)
                    {
                        foreach (var file in AdditionalImages)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string extension = Path.GetExtension(file.FileName).ToLower();
                                string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid() + extension;
                                string path = Path.Combine(Server.MapPath("~/Image/product-image"), fileName);
                                file.SaveAs(path);
                                // lưu ảnh phụ vào bảng ImageProducts
                                db.ImageProducts.Add(new ImageProduct
                                {
                                    ProductsID = model.Product.ProductID,
                                    ImageURL = fileName
                                });
                            }
                        }
                    }
                    // lưu thay đổi
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MangeProduct");
                }
            }
            // nếu có lỗi ,truyền lại dữ liệu cần thiết cho view 
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", model.Product.CategoryID);
            model.Colors = db.Colors.ToList();
            model.AdditionalImagesUrls = db.ImageProducts
                .Where(img => img.ProductsID == model.Product.ProductID)
                .Select(img => img.ImageURL)
                .ToList(); // gán lại ảnh phụ khi có lỗi 
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProducts(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound();
                }
                // xóa ảnh chính của sản phẩm khỏi thư mục 
                var mainImagePath = Path.Combine(Server.MapPath("~/image/product-image"), product.ImageURL);
                if (System.IO.File.Exists(mainImagePath))
                {
                    System.IO.File.Delete(mainImagePath);
                }
                // xóa các ghi bản ghi liên quan trong ProductStocks
                var productStock = db.ProductStocks
                    .Where(ps => ps.ProductID == id).ToList();
                foreach (var stock in productStock)
                {
                    db.ProductStocks.Remove(stock);
                }
                // xóa các ảnh phụ trong ImageProducts
                var additionalImages = db.ImageProducts.Where(ip => ip.ProductsID == id).ToList();
                foreach (var image in additionalImages)
                {
                    var additionalImagePath = Path.Combine(Server.MapPath("~/image/product-image"), image.ImageURL);
                    if (System.IO.File.Exists(additionalImagePath))
                    {
                        System.IO.File.Delete(additionalImagePath);
                    }
                    db.ImageProducts.Remove(image);
                }
                // xóa sản phẩm 
                db.Products.Remove(product);
                db.SaveChanges();
                // Lưu thông báo thành công vào TempData
                TempData["SuccessMessage"] = "sản phẩm đã được xóa thành công ";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xóa sản phẩm " + ex.Message);
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm vì có dữ liệu liên quan trong đơn hàng ";
            }
            return RedirectToAction("MangeProducts");

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid) // kiểm tra dữ liệu trong model có hợp lệ hay không 

            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("ManageCategory");
            }

            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(category);
        }
        public ActionResult CreateCategory()
        {
            return View();
        }
        public ActionResult EditCategory(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra nếu là yêu cầu AJAX
            if (Request.IsAjaxRequest())
            {
                // Trả về dữ liệu dưới dạng JSON để hiển thị trong modal
                return Json(new { CategoryID = category.CategoryID, CategoryName = category.CategoryName }, JsonRequestBehavior.AllowGet);
            }

            return View(category); // Trả về view khi truy cập trực tiếp
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
                    p.CategoryID
                })
                .FirstOrDefault();
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = product }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult AddPromotion()
        {
            return View();
        }
        //[HttpPost]
        /* public ActionResult AddPromotion(string promotionName, string description, decimal? discountAmount, decimal? discountPercentage, DateTime startDate, DateTime endDate)
         {
             // kiểm tra tính hợp lệ của dữ liệu 
             if (string.IsNullOrEmpty(promotionName) || startDate >= endDate)
             {
                 ModelState.AddModelError("", "Invalid promotion data");
                 return View();

             }
             // tạo đối tượng promotion mới 
             var newPromotion = new Promotion
             {
                 PromotionName = promotionName,
                 Description = description,
                 DiscountAmount = discountAmount,
                 DiscountPercentage = discountPercentage,
                 StartDate = startDate,
                 EndDate = endDate

             };
             // thêm chương trình khuyeesnn mãi vaof bảng Promotion
             db.Promotions.Add(newPromotion);
             db.SaveChanges();
             // chuyển hướng về danh sách hoặc trang khác nếu cần 
             return RedirectToAction("PromotionList");// hoặc bất kỳ action nào bn muốn chuyển tới 
         }*/
        //bản tối uu hơn có thêm bước kiểm tra 
        public ActionResult AddPromotion(string promotionName, string description, decimal? discountAmount, decimal? discountPercentage, DateTime startDate, DateTime endDate, int? productId = null)
        {
            // kiểm tra tính hợp lệ của dữ liệu 
            if (string.IsNullOrEmpty(promotionName) || startDate >= endDate)
            {
                ModelState.AddModelError("", "Invalid promotion data");
                return View();

            }
            var newPromotion = new Promotion
            {
                PromotionName = promotionName,
                Description = description,
                DiscountAmount = discountAmount,
                DiscountPercentage = discountPercentage,
                StartDate = startDate,
                EndDate = endDate,

            };
            // thêm chương trình khuyesn mãi vào bảng promotion
            db.Promotions.Add(newPromotion);
            db.SaveChanges();
            // nếu có ProductId hợp lệ thêm vào bảng ProducPromotions để liên kết sảm phẩm tới ct khuến mãi
            if (productId.HasValue)
            {
                var productPromotion = new ProductPromotion
                {
                    ProductID = productId.Value,
                    PromotionID = newPromotion.PromotionID
                };
                db.ProductPromotions.Add(productPromotion);
                db.SaveChanges();

            }
            // chuyển hướng về danh sách chương trình khuyến mãi hoặc trang khác nếu cần 
            return RedirectToAction("PromotionList");

        }
        [HttpGet]
        public ActionResult PromotionList()
        {
            var promotions = db.Promotions.ToList();
            return View(promotions);
        }
        [HttpGet]
        public ActionResult EditPromotion(int id )
        {
            var promotion = db.Promotions
                .Where(p => p.PromotionID == id)
            .Select(p => new PromotionViewModel
            {
                PromotionID = p.PromotionID,
                PromotionName = p.PromotionName,
                Description = p.Description,
                DiscountAmount = p.DiscountAmount,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).FirstOrDefault();

            if(promotion == null)
            {
                return HttpNotFound();
            }    

            return View(promotion);
        }
        [HttpPost]
        public ActionResult EditPromotion(PromotionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var promotion =db.Promotions.Find(model.PromotionID);// liên kết khóa chính đến database
                if (promotion == null)
                {
                    return HttpNotFound();
                }
                // cập nhật các trường 
                promotion.PromotionName = model.PromotionName;  
                promotion.Description = model.Description;
                promotion.DiscountAmount = model.DiscountAmount;
                promotion.DiscountPercentage = model.DiscountPercentage;    
                promotion.StartDate = model.StartDate;  
                promotion.EndDate = model.EndDate;
                db.SaveChanges();
                // chuyển về danh sách khuyến mãi sau khi lưu 
                return RedirectToAction("PromotionList","Admin");

            }
            return View(model);
        }
        
        public ActionResult Orders()
        {
            return View();
        }
        //Get: Inventory
        public ActionResult MangeInventory()
        {
            // lấy thông tin sản pghaamr và tồn kho 
            var inventoryData = db.ProductStocks
                .Select(ps => new Models.InventoryAddStockViewModel
                {
                    ProductStockID = ps.ProductStockID,
                    ProductID = (int)ps.ProductID,
                    ProductName = ps.Product.ProductName,
                    MainImageUrl = ps.Product.ImageURL,
                    Size = ps.Size.Size1,// size1 là sizeValue
                    Color = ps.Color.Color1,
                    StockQuantity = (int)ps.Quantity
                }).ToList();
            return View(inventoryData); 
        }

        public ActionResult AddStock(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            // tìm sản phẩm theo ProductStockID
            var productStock = db.ProductStocks
                .FirstOrDefault(ps => ps.ProductStockID == id);
            if (productStock == null)
            {
                return Json(new { success = false, message = " Không tìm thầy sản phẩm trong kho " });
            }
            var model = new InventoryAddStockViewModel
            {
                ProductStockID= productStock.ProductStockID,
                ProductID = (int)productStock.ProductID,
                ProductName = productStock.Product.ProductName,
                Size = productStock.Size.Size1, 
                CurrentStockQuantity = (int)productStock.Quantity
            };
            return PartialView("_AddStock",model);
         }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStock(InventoryAddStockViewModel model)
        {
            if (ModelState.IsValid)
            {
                var productStock = db.ProductStocks.FirstOrDefault(ps => ps.ProductStockID == model.ProductStockID);
                if (productStock != null)
                {
                    if(model.QuantityAdded <= 0)
                    {
                        return Json(new { succes = false, message = "Số lượng nhập kho phải lớn hơn 0." });
                    }
                    // câp nhật số lượng kho 
                    productStock.Quantity = model.QuantityAdded;
                    db.Entry(productStock).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new {success=false,message="không tìm thấy sản phẩm trong kho "});
                }                                    
            }
            return Json(new { success = false, message = "Không thể cập nhật số lượng kho" });
        }
        public ActionResult OrderHistory()
        {
            using(var db = new WebThoiTrangEntities())
            {
                var orders = (from o in db.Orders
                              join u in db.Users on o.UserID equals u.IdUser
                              select new NewOrderHistoryViewModel
                              {
                                  UserID = o.UserID,
                                  OrderID = o.OrderID,
                                  OrderDate = o.OrderDate,
                                  Status = o.status,
                                  Block = o.Block,
                                  Town = o.Town,
                                  Province = o.Province,
                                  FullName = (string)o.FullName,
                                  Phone = o.phone,
                                  Email = u.Email,//lấy thông tin tuwfg bảng Users
                                  Ip = u.Ip, // lấy thông tin Ip từ bảng Users
                                  OrderDetails = db.OrderDetails
                                  .Where(od => od.OrderID == o.OrderID)
                                  .Select(od => new NewOrderDetailViewModel
                                  {
                                      ProductID = od.ProductID,
                                      ProductName = db.Products.FirstOrDefault(p => p.ProductID == od.ProductID).ProductName,
                                      Quantity = od.Quantity,
                                      UnitPrice = od.UnitPrice
                                  }).ToList()


                              }).ToList();//lấy danh sách đơn hàng 
                return View(orders);// trả về view với danh sách đơn hàng đã được kết hợp thông tin từ Users
            }               
        }
        public ActionResult OrderHistorySuccess()
        {
            using (var db = new WebThoiTrangEntities())
            {
                var orders = (from o in db.Orders
                              join u in db.Users on o.UserID equals u.IdUser//kết jopwj bảng orders và users
                              where o.status != 0 // lọc những đon hàng có status khác 0
                              select new NewOrderHistoryViewModel
                              {
                                  UserID = o.UserID,
                                  OrderID = o.OrderID,
                                  OrderDate = o.OrderDate,
                                  Status = o.status,
                                  Price = (int)o.price,
                                  FullName = (string)o.FullName,
                                  SpecificAddress = o.SpecificAddress,
                                  Block = o.Block,
                                  Town = o.Town,
                                  Province = o.Province,
                                  Phone = o.phone,
                                  Email = u.Email,// lấy thông tin Emails từ bảng Users
                                  Ip = u.Ip, // lấy thông tin từ bảng users
                                  OrderDetails = db.OrderDetails
                                  .Where(od => od.OrderID == o.OrderID)
                                  .Select(od => new NewOrderDetailViewModel
                                  {
                                      ProductID = od.ProductID,
                                      ProductName = db.Products.FirstOrDefault(p => p.ProductID == od.ProductID).ProductName,
                                      Quantity = od.Quantity,   
                                      UnitPrice = od.UnitPrice,
                                  }).ToList()


                              }
                              ).ToList();
                return View(orders );
            }    
        }
        public ActionResult OrderDetailHistory(int orderID)
        {
            using(var db = new WebThoiTrangEntities())
            {
                // truy vấn đơn hàng và kết hợp với thông tin người dùng 
                var order = (from o in db.Orders
                             join u in db.Users on o.UserID equals u.IdUser // kết hợp bảng orders và users
                             where o.OrderID == orderID
                             select new NewOrderHistoryViewModel
                             {
                                 UserID = o.UserID,
                                 OrderID = o.OrderID,
                                 OrderDate = o.OrderDate,
                                 Status = o.status,
                                 Price = (int)o.price,
                                 FullName = (string)o.FullName,
                                 SpecificAddress = o.SpecificAddress,
                                 Block = o.Block,
                                 Town = o.Town,
                                 Province = o.Province,
                                 Phone = o.phone,
                                 Email = u.Email,// lấy thông tin Emails từ bảng Users
                                 Ip = u.Ip, // lấy thông tin từ bảng users
                                 OrderDetails =o.OrderDetails
                                  .Select(od => new NewOrderDetailViewModel
                                  {
                                      ProductID =od.ProductID,
                                      ProductName = db.Products
                                      .Where (p=> p.ProductID == od.ProductID)
                                      .Select(p => p.ProductName)
                                      .FirstOrDefault()??"Unknown Product",// nếu không tìm thấy sản phẩm gán ra giá trị mặc định 
                                      Quantity = od.Quantity,   
                                      UnitPrice = od.UnitPrice,

                                  }).ToList()
                             }
                             ).FirstOrDefault();
                if(order ==null)
                {
                    return HttpNotFound("Đơn hàng không tồn tại");
                }    
                return View(order);
            }    
        }
        //API để lấy thoog tin thống kê thu nhập theo tháng 
        [HttpGet]
        public JsonResult GetEaringsByMonth()
        {
            try
            {
                var months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "August", "Sep", "Oct", "Nov", "Dec" };
                //truy vấn thu nhập theo tháng từ bảng Orders 
                var earnings = db.Orders
                    .Where(o => o.OrderDate.Year == 2025)
                    .GroupBy(o => new { Year = o.OrderDate.Year, Month = o.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalEarnings = g.Sum(o => o.price)
                    })
                    .OrderBy(g => g.Month)
                    .ToList();
                //tạo dữ liệu cho các tháng từ 1-11
                var result = new
                {
                    months = months.Take(11).ToList(),// lấy 10 tháng đầu 
                    earnings = months.Take(11).Select(month => earnings.FirstOrDefault(e => e.Month == Array.IndexOf(months, month) + 1)?.TotalEarnings ?? 0).ToList()

                };
                return Json(result, JsonRequestBehavior.AllowGet);// Trả về dữ liệu dạng JSON
            }
            catch(Exception ex)
             {
                return Json(new {error = ex.Message},JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult Users()
        {
            //kết hợp bảng Users và AddressUser
            var users = (from u in db.Users
                         join a in db.AddressUsers on u.IdUser equals a.IdUser
                         select new UserAdminViewModel
                         {
                             UserID = u.IdUser,
                             Email = u.Email,
                             Username = a.FullName_,
                             Address = a.SpecificAddress,//kết hợp địa chỉ từ bảng AddressUser
                             PhoneNumber = a.Phone,// số điện thoại từ AddressUser
                             IPAddress = u.Ip
                         }).ToList();
            return View(users);
        }


        

    }
}
    

