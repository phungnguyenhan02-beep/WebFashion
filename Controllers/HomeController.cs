using LT_WebThoiTrang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LT_WebThoiTrang.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebThoiTrangEntities _context;

        public HomeController()
        {
            _context = new WebThoiTrangEntities();
        }

        public ActionResult Index()
        {
            var featuredProducts = _context.Products.Take(8).ToList();
            var newProducts = _context.Products.OrderByDescending(p => p.ProductID).Take(8).ToList();
            return View(new HomeViewModel
            {
                FeaturedProducts = featuredProducts,
                NewProducts = newProducts
            });
        }
    }
}