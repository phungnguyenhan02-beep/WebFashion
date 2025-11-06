using LT_WebThoiTrang.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

public class CartsController : Controller
{
    private readonly WebThoiTrangEntities _context;

    public CartsController()
    {
        _context = new WebThoiTrangEntities();
    }

    public ActionResult Index()
    {
        var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
        return View(cart);
    }

    public ActionResult AddToCart(int productId, int quantity = 1, string size = null)
    {
        var product = _context.Products.Find(productId);
        if (product == null)
            return HttpNotFound();

        var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

        var existing = cart.FirstOrDefault(c => c.Product.ProductID == productId && c.Size == size);
        if (existing != null)
            existing.Quantity += quantity;
        else
            cart.Add(new CartItem { Product = product, Quantity = quantity, Size = size });

        Session["Cart"] = cart;
        return RedirectToAction("Index");
    }

    public ActionResult Remove(int id)
    {
        var cart = Session["Cart"] as List<CartItem>;
        if (cart != null)
        {
            var item = cart.FirstOrDefault(c => c.Product.ProductID == id);
            if (item != null) cart.Remove(item);
            Session["Cart"] = cart;
        }
        return RedirectToAction("Index");
    }
}

public class CartItem
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public string Size { get; set; }
}
