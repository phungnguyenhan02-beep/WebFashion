using LT_WebThoiTrang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LT_WebThoiTrang.Controllers
{
    public class UserAddressesController : Controller
    {
        // GET: UserAddresses


        public ActionResult Index()
        {
            string email = Session["Email"] as string;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Sign_in", "Account");
            }

            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return HttpNotFound();
                }

                var address = db.AddressUsers.FirstOrDefault(a => a.IdUser == user.IdUser);
                var viewModel = new AddressUserViewModel
                {
                    IdUser = user.IdUser,
                    FullName = address?.FullName_ ?? "",
                    Phone = address?.Phone ?? "",
                    Province = address?.Province ?? "",
                    Town = address?.Town ?? "",
                    Block = address?.Block ?? "",
                    SpecificAddress = address?.SpecificAddress ?? ""
                };
                ViewBag.UserEmail = email;
                return View(viewModel);
            }
        }
        [HttpPost]
        public ActionResult Update(AddressUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string email = Session["Email"] as string;
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToAction("Sign_in", "Account");
                }
                using (WebThoiTrangEntities db = new WebThoiTrangEntities())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                    var address = db.AddressUsers.FirstOrDefault(a => a.IdUser == user.IdUser);
                    if (address == null)
                    {
                        address = new AddressUser { IdUser = user.IdUser };
                        db.AddressUsers.Add(address);
                    }
                    address.FullName_ = viewModel.FullName;
                    address.Phone = viewModel.Phone;
                    address.Province = viewModel.Province;
                    address.Town = viewModel.Town;
                    address.Block = viewModel.Block;
                    address.SpecificAddress = viewModel.SpecificAddress;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
                }
            }

            return RedirectToAction("Index");
        }
    }
}
            


        
    

    