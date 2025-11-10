using LT_WebThoiTrang.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System;

public class CartsController : Controller
{
    private WebThoiTrangEntities db = new WebThoiTrangEntities();
    // lấy giỏ hàng từ session
    private Cart GetCartFromSession()
    {
        var cart = Session["Cart"] as Cart;
        if (cart == null)
        {
            cart = new Cart();  // Tạo giỏ hàng mới nếu không có
            Session["Cart"] = cart;
        }

        return cart;
    }

    // Thêm sản phẩm vào giỏ hàng
    public ActionResult AddToCart(int productId, int quantity, string size)
    {
        // Kiểm tra xem người dùng đã đăng nhập chưa
        if (Session["Email"] == null)
        {
            // Nếu chưa đăng nhập, chuyển hướng về trang đăng nhập
            return RedirectToAction("Sign_in", "Account");
        }

        var cart = GetCartFromSession();
        using (var db = new WebThoiTrangEntities())
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product != null)
            {
                // Thêm sản phẩm vào giỏ với kích thước đã chọn
                cart.AddItem(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price =(int) product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageURL,
                    Size = size,
                    DateAdded = DateTime.UtcNow  // Ghi lại thời điểm thêm vào
                });
            }
        }

        // Lưu tổng số lượng giỏ hàng vào Session
        Session["CartQuantity"] = cart.GetTotalQuantity();

        // Chuyển hướng lại trang ProductPage với sản phẩm hiện tại
        return RedirectToAction("ProductPage", "ProductPage", new { id = productId });
    }



    // Hiển thị giỏ hàng
    public ActionResult ViewCart()
    {
        var cart = GetCartFromSession();
        var sortedItems = cart.Items.OrderByDescending(item => item.DateAdded).ToList();  // Sắp xếp từ mới nhất
        cart.Items = sortedItems;  // Gán danh sách đã sắp xếp lại
        return View(cart);
    }


    // Cập nhật số lượng sản phẩm trong giỏ hàng
    [HttpPost]
    public ActionResult UpdateCartItem(int productId, string size, int quantity)
    {
        var cart = GetCartFromSession();
        cart.UpdateItem(productId, size, quantity);  // Cập nhật số lượng sản phẩm theo size
                                                     // Cập nhật số lượng giỏ hàng vào Session
        Session["CartQuantity"] = cart.GetTotalQuantity();
        return RedirectToAction("ViewCart");
    }


    // Xóa sản phẩm khỏi giỏ hàng
    [HttpPost]
    public ActionResult RemoveCartItem(int productId)
    {
        var cart = GetCartFromSession();
        cart.RemoveItem(productId);
        // Cập nhật số lượng giỏ hàng vào Session
        Session["CartQuantity"] = cart.GetTotalQuantity();
        return RedirectToAction("ViewCart");
    }

    // Thanh toán
    public ActionResult Checkout()
    {
        // Kiểm tra IdUser trong session
        int? userId = Session["IdUser"] as int?;
        if (!userId.HasValue)
        {
            // Nếu không có IdUser, chuyển hướng đến trang đăng nhập
            return RedirectToAction("Sign_in", "Account");
        }

        // Lấy thông tin giỏ hàng từ session
        var cart = GetCartFromSession();
        var cartItems = cart.Items.ToList();
        // Kiểm tra nếu giỏ hàng trống
        if (!cartItems.Any())
        {
            TempData["Message"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán.";
            return RedirectToAction("ViewCart", "Carts"); // Chuyển hướng về trang giỏ hàng
        }
        using (WebThoiTrangEntities db = new WebThoiTrangEntities())
        {
            // Lấy thông tin địa chỉ của người dùng từ AddressUser
            var userAddress = db.AddressUsers.FirstOrDefault(a => a.IdUser == userId.Value);
            if (userAddress == null || string.IsNullOrEmpty(userAddress.FullName_) ||
            string.IsNullOrEmpty(userAddress.Phone) ||
            string.IsNullOrEmpty(userAddress.Province) ||
            string.IsNullOrEmpty(userAddress.Town) ||
            string.IsNullOrEmpty(userAddress.Block) ||
            string.IsNullOrEmpty(userAddress.SpecificAddress))
            {
                // Nếu địa chỉ không có hoặc còn thiếu thông tin, chuyển hướng người dùng đến trang cập nhật địa chỉ
                TempData["Message"] = "Vui lòng nhập đầy đủ thông tin địa chỉ trước khi thanh toán.";
                return RedirectToAction("Index", "UserAddresss");
            }


            // Tính tổng số tiền giỏ hàng
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Price);

            // Chuyển thông tin cho View
            var checkoutViewModel = new CheckOutViewModels
            {
                Address = userAddress,
                CartItems = cartItems,
                TotalAmount = totalAmount
            };

            return View(checkoutViewModel);
        }
    }

    [HttpPost]
    public ActionResult Checkout(CheckOutViewModels model)
    {
        int? userId = Session["IdUser"] as int?;
        if (!userId.HasValue)
        {
            return RedirectToAction("Sign_in", "Account");
        }

        using (WebThoiTrangEntities db = new WebThoiTrangEntities())
        {
            // Cập nhật địa chỉ người dùng
            var userAddress = db.AddressUsers.FirstOrDefault(a => a.IdUser == userId.Value);
            if (userAddress == null)
            {
                return RedirectToAction("Index", "UserAddresss");
            }

            userAddress.FullName_ = model.Address.FullName_;
            userAddress.Phone = model.Address.Phone;
            userAddress.Province = model.Address.Province;
            userAddress.Town = model.Address.Town;
            userAddress.Block = model.Address.Block;
            userAddress.SpecificAddress = model.Address.SpecificAddress;
            db.SaveChanges();

            // Lấy giỏ hàng và tính tổng số tiền
            var cart = GetCartFromSession();
            var cartItems = cart.Items.ToList();
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.Price);

            // Tạo đơn hàng mới
            var newOrder = new Order
            {
                UserID = userId.Value,
                FullName = userAddress.FullName_,
                SpecificAddress = userAddress.SpecificAddress,
                Block = userAddress.Block,
                Town = userAddress.Town,
                Province = userAddress.Province,
                phone = userAddress.Phone,
                OrderDate = DateTime.UtcNow.ToLocalTime(),
                status = 0,
                price = (int)totalAmount
            };
            db.Orders.Add(newOrder);
            db.SaveChanges();

            // Tạo chi tiết đơn hàng
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = newOrder.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    size = item.Size,
                };
                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges();

            // Xóa giỏ hàng
            ClearCart(userId.Value);

            // Chuyển hướng đến Payment action với orderId và totalAmount
            return RedirectToAction("Payment", "Carts", new { orderId = newOrder.OrderID, totalAmount = totalAmount });
        }
    }

    // Xóa giỏ hàng
    private void ClearCart(int userId)
    {
        Session["Cart"] = null;
        Session["CartQuantity"] = 0;
    }

    // Xác nhận đơn hàng
    public ActionResult OrderConfirmation()
    {
        return View();
    }
    public ActionResult OrderHistory()
    {
        int? userId = Session["IdUser"] as int?;
        if (!userId.HasValue)
        {
            return RedirectToAction("Sign_in", "Account");
        }

        using (var db = new WebThoiTrangEntities())
        {
            var orders = db.Orders
                           .Where(o => o.UserID == userId.Value)
                           .OrderByDescending(o => o.OrderID) // Sắp xếp theo ID từ lớn đến bé
                           .Select(o => new OrderHistoryViewModel
                           {
                               OrderID = o.OrderID,
                               OrderDate = o.OrderDate,
                               Status = o.status,
                               Price = (int)o.price,
                               SpecificAddress = o.SpecificAddress,
                               Block = o.Block,
                               Town = o.Town,
                               Province = o.Province,
                               Phone = o.phone,
                               OrderDetails = db.OrderDetails
                                                .Where(od => od.OrderID == o.OrderID)
                                                .Select(od => new OrderDetailViewModel
                                                {
                                                    ProductID = od.ProductID,
                                                    ProductName = db.Products.FirstOrDefault(p => p.ProductID == od.ProductID).ProductName,
                                                    Quantity = od.Quantity,
                                                    UnitPrice = od.UnitPrice,
                                                    Size = od.size
                                                }).ToList()
                           }).ToList();

            return View(orders);
        }
    }
    public ActionResult OrderDetail(int orderId)
    {
        using (var db = new WebThoiTrangEntities())
        {
            // Tìm đơn hàng theo orderId
            var order = db.Orders
                .Where(o => o.OrderID == orderId)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderID = o.OrderID,
                    OrderDate = o.OrderDate,
                    Status = o.status,
                    Price = (decimal)o.price,
                    SpecificAddress = o.SpecificAddress,
                    Block = o.Block,
                    Town = o.Town,
                    Province = o.Province,
                    Phone = o.phone,
                    // Lấy danh sách OrderDetails và thêm thông tin sản phẩm từ bảng Product
                    OrderDetails = o.OrderDetails
                        .Select(od => new OrderDetailViewModel
                        {
                            ProductID = od.ProductID,
                            Quantity = od.Quantity,
                            UnitPrice = od.UnitPrice,
                            Size = od.size,
                            ProductName = db.Products
                                            .Where(p => p.ProductID == od.ProductID)
                                            .Select(p => p.ProductName)
                                            .FirstOrDefault()  // Lấy tên sản phẩm
                        }).ToList()
                }).FirstOrDefault();

            // Kiểm tra nếu không tìm thấy đơn hàng
            if (order == null)
            {
                return HttpNotFound();  // Trả về lỗi 404 nếu không tìm thấy đơn hàng
            }

            return View(order);
        }
    }

    public ActionResult Payment(int orderId, int totalAmount)
    {
        ViewBag.BankName = "MB";
        ViewBag.AccountNumber = "0344264533";
        ViewBag.AccountName = "Phùng Nguyên Hãn";
        ViewBag.TransactionId = $"Levents{orderId}";
        ViewBag.Amount = totalAmount; // Đảm bảo truyền đúng totalAmount
        ViewBag.OrderID = orderId;
        return View();
    }
    [HttpGet]
    public JsonResult CheckOrderStatus(int orderId)
    {
        using (var db = new WebThoiTrangEntities())
        {
            var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return Json(new { status = -1, statusText = "Không tìm thấy đơn hàng" }, JsonRequestBehavior.AllowGet);
            }

            // Trả về JSON trạng thái
            return Json(new { status = order.status, statusText = order.status == 1 ? "Hoàn tất" : "Đang xử lý" }, JsonRequestBehavior.AllowGet);
        }
    }
    // Chức năng "Mua Ngay"
    public ActionResult BuyNow(int productId, string size)
    {
        // Kiểm tra xem người dùng đã đăng nhập chưa
        if (Session["Email"] == null)
        {
            // Nếu chưa đăng nhập, chuyển hướng về trang đăng nhập
            return RedirectToAction("Sign_in", "Account");
        }

        using (var db = new WebThoiTrangEntities ())
        {
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product != null)
            {
                // Tạo giỏ hàng tạm thời
                var cart = new Cart();
                cart.AddItem(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price = (int)product.Price,
                    Quantity = 1,  // Số lượng mặc định là 1
                    ImageUrl = product.ImageURL,
                    Size = size,
                    DateAdded = DateTime.UtcNow
                });

                // Tính tổng số tiền cho đơn hàng
                int totalAmount = (int)cart.Items.Sum(item => item.Quantity * item.Price);

                // Kiểm tra thông tin địa chỉ người dùng
                int? userId = Session["IdUser"] as int?;
                if (!userId.HasValue)
                {
                    return RedirectToAction("Sign_in", "Account");
                }

                // Lấy thông tin địa chỉ người dùng từ AddressUser
                var userAddress = db.AddressUsers.FirstOrDefault(a => a.IdUser == userId.Value);
                if (userAddress == null || string.IsNullOrEmpty(userAddress.FullName_) ||
                    string.IsNullOrEmpty(userAddress.Phone) ||
                    string.IsNullOrEmpty(userAddress.Province) ||
                    string.IsNullOrEmpty(userAddress.Town) ||
                    string.IsNullOrEmpty(userAddress.Block) ||
                    string.IsNullOrEmpty(userAddress.SpecificAddress))
                {
                    TempData["Message"] = "Vui lòng nhập đầy đủ thông tin địa chỉ trước khi thanh toán.";
                    return RedirectToAction("Index", "UserAddresss");
                }

                // Tạo đơn hàng mới
                var newOrder = new Order
                {
                    UserID = userId.Value,
                    FullName = userAddress.FullName_,
                    SpecificAddress = userAddress.SpecificAddress,
                    Block = userAddress.Block,
                    Town = userAddress.Town,
                    Province = userAddress.Province,
                    phone = userAddress.Phone,
                    OrderDate = DateTime.UtcNow.ToLocalTime(),
                    status = 0,
                    price = totalAmount
                };
                db.Orders.Add(newOrder);
                db.SaveChanges();

                // Tạo chi tiết đơn hàng
                foreach (var item in cart.Items)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderID = newOrder.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        size = item.Size
                    };
                    db.OrderDetails.Add(orderDetail);
                }
                db.SaveChanges();

                // Chuyển hướng đến trang thanh toán
                return RedirectToAction("Payment", "Carts", new { orderId = newOrder.OrderID, totalAmount = totalAmount });
            }
        }

        return RedirectToAction("Home_page", "HomePage");
    }
}

