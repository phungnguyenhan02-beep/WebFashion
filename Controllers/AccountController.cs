using LT_WebThoiTrang.Models;
using LT_WebThoiTrang.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace LT_WebThoiTrang.Controllers
{
    public class AccountController : Controller
    {

        // GET: Account

        public ActionResult Sign_in()
        {
            if (Session["Email"] != null)
            {
                return RedirectToAction("Home_page", "HomePage");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Sign_in(string user, string password)
        {

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập cả email và mật khẩu");
                return View();
            }
            if (user.Length < 10 || user.Length > 35)
            {
                ModelState.AddModelError("", "Email phải có độ dài kí tự từ 10 đến 35");
                return View();
            }
            if (password.Length < 6 || password.Length > 40)
            {
                ModelState.AddModelError("", "Password phải có độ dài kí tự từ 6 đến 40");
                return View();
            }
            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                string md5password = UserServices.GetMd5Hash(password);
                // Tìm người dùng trong cơ sở dữ liệu
                var userInDb = db.Users.FirstOrDefault(u => u.Email.ToLower() == user.ToLower() && u.PasswordHash == md5password);

                if (userInDb != null)
                {
                    if (userInDb.IsEmailVerified == false)
                    {
                        ModelState.AddModelError("", "Vui lòng vào email để xác thực tài khoản");
                        return View();
                    }

                    Session["Email"] = userInDb.Email;
                    Session["IdUser"] = userInDb.IdUser;
                    Session["IsAdmin"] = userInDb.IsAdmin == 1;
                    return RedirectToAction("Home_page", "HomePage");
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
                    return View();
                }


            }

        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Sign_in");
        }
        // GET: Register
        public ActionResult Register()
        {
            if (Session["Email"] != null)
            {
                return RedirectToAction("Home_page", "HomePage");
            }
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Register(string user, string password, string confirmpassword)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập cả email và mật khẩu");
                return View();
            }
            if (user.Length < 10 || user.Length > 35)
            {
                ModelState.AddModelError("", "Email phải có độ dài kí tự từ 10 đến 35");
                return View();
            }
            if (password.Length < 6 || password.Length > 40)
            {
                ModelState.AddModelError("", "Password phải có độ dài kí tự từ 6 đến 40");
                return View();
            }
            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                var userInDb = db.Users.FirstOrDefault(u => u.Email.ToLower() == user.ToLower());
                if (userInDb == null)
                {
                    if (password == confirmpassword)
                    {
                        string token = UserServices.GetMd5Hash(UserServices.GenerateRandomCode(15));
                        string Ip = UserServices.GetUserIP();
                        string md5password = UserServices.GetMd5Hash(password);

                        var _user = new User
                        {
                            Email = user,
                            PasswordHash = md5password,
                            TokenPassword = token,
                            IsEmailVerified = false,
                            Ip = Ip,
                            Stt = 0,
                            IsAdmin = 0
                        };
                        db.Users.Add(_user);
                        db.SaveChanges();
                        // Lưu thông tin địa chỉ vào bảng AddressUser
                        // Tạo AddressUser mới và liên kết với IdUser của người dùng mới tạo
                        var addressUser = new AddressUser
                        {
                            IdUser = _user.IdUser, // Gán IdUser từ User
                            FullName_ = "", // Bạn có thể lấy thông tin này từ model nếu có
                            Phone = "", // Bạn có thể lấy thông tin này từ model nếu có
                            Province = "", // Tương tự như trên
                            Town = "",
                            Block = "",
                            SpecificAddress = ""
                        };

                        db.AddressUsers.Add(addressUser);
                        db.SaveChanges();
                        await SendEmail.EmailSenderAsync(user, "Xác thực tài khoản", $"https://hangmusports.site/verify/{token}");
                        ModelState.AddModelError("", "Đăng kí thành công,vui lòng vào email để xác thực tài khoản");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu confirm không đúng với mật khẩu đã nhập");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản đã được sử dụng vui lòng nhập tài khoản khác");
                }
            }

            return View();
        }
        public ActionResult ForgetPassword()
        {
            if (Session["Email"] != null)
            {
                return RedirectToAction("Home_page", "HomePage");
            }
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ForgetPassword(string recipientEmail, string subject, string body)
        {

            try
            {
                // Nếu bạn muốn sử dụng email từ người dùng nhập vào
                if (string.IsNullOrEmpty(recipientEmail))
                {
                    ModelState.AddModelError("", "Vui lòng nhập Email của người nhận");
                    return View();
                }


                // Kiểm tra xem email có tồn tại trong cơ sở dữ liệu không
                using (WebThoiTrangEntities db = new WebThoiTrangEntities())
                {
                    var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == recipientEmail.ToLower());
                    if (userInDb == null)
                    {
                        ModelState.AddModelError("", "Email không tồn tại.");
                        return View();
                    }

                    // Gán cứng subject
                    subject = "Xác nhận yêu cầu"; // Gán giá trị cho subject

                    // Tạo body ngẫu nhiên với 6 chữ số
                    body = UserServices.GenerateRandomCode(6); // Tạo body ngẫu nhiên
                                                               // Gửi email
                    await SendEmail.EmailSenderAsync(recipientEmail, subject, body);
                    // Cập nhật TokenPassword và CreatedAt cho người dùng hiện có
                    userInDb.TokenPassword = body;
                    userInDb.CreatedAt = DateTime.UtcNow.AddMinutes(5);

                    await db.SaveChangesAsync();
                }
                // Chuyển hướng đến VerifyResetCode với email đã gửi
                return RedirectToAction("VerifyResetCode", new { email = recipientEmail });
            }
            catch (Exception ex)
            {
                return Content("Có lỗi xảy ra khi gửi email: " + ex.Message);
            }
        }
        public ActionResult VerifyResetCode()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> VerifyResetCode(string email, string resetCode)
        {
            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                // Tìm người dùng với email và mã xác nhận khớp
                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.TokenPassword == resetCode);

                // Kiểm tra xem mã xác nhận có tồn tại, hợp lệ, và chưa hết hạn không
                if (user == null || user.TokenPassword != resetCode || user.CreatedAt == null || user.CreatedAt < DateTime.UtcNow)
                {
                    ModelState.AddModelError("", "Mã xác nhận không hợp lệ hoặc đã hết hạn.");
                    return View("VerifyResetCode"); // Chuyển hướng về trang VerifyResetCode nếu mã không hợp lệ
                }

                // Nếu mã xác nhận hợp lệ, chuyển đến trang ResetPassword và truyền email và resetCode
                return RedirectToAction("ResetPassword", new { email = email, resetCode = resetCode });
            }
        }


        public ActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ResetPassword(string email, string resetCode, string newPassword, string confirmpassword)
        {
            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                // Tìm người dùng với email và mã xác nhận
                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.TokenPassword == resetCode);
                // Cập nhật mật khẩu mới
                if (newPassword == confirmpassword)
                {
                    user.PasswordHash = UserServices.GetMd5Hash(newPassword); // Mã hóa mật khẩu mới
                    user.TokenPassword = null; // Xóa mã xác nhận để không thể sử dụng lại
                    user.CreatedAt = null; // Xóa thời gian tạo mã
                    await db.SaveChangesAsync();
                    ModelState.AddModelError("", "Mật khẩu đã được đặt lại thành công.");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu không trùng khớp.");
                }

                return View();
            }
        }
        public ActionResult Verify(string code)
        {
            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.TokenPassword == code);

                if (user != null)
                {
                    user.TokenPassword = null;
                    user.IsEmailVerified = true;
                    db.SaveChanges();
                    return Content("bạn đã xác thực email thành công");
                }
                else
                {
                    // Chuyển hướng đến trang lỗi nếu mã không hợp lệ
                    return RedirectToAction("Error", "Error", new { statusCode = 404, message = "Liên kết không tồn tại" });
                }
            }
        }
        // GET: ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: ChangePassword
        [HttpPost]
        public async Task<ActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            int? userId = Session["IdUser"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Sign_in");
            }

            using (WebThoiTrangEntities db = new WebThoiTrangEntities())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.IdUser == userId.Value);
                if (user == null)
                {
                    ModelState.AddModelError("", "Người dùng không tồn tại.");
                    return View();
                }

                // Xác thực mật khẩu hiện tại
                if (user.PasswordHash != UserServices.GetMd5Hash(currentPassword))
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                    return View();
                }
                if (user.PasswordHash == UserServices.GetMd5Hash(newPassword))
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không được trùng với mật khẩu thay đổi.");
                    return View();
                }
                // Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                    return View();
                }

                // Cập nhật mật khẩu mới
                user.PasswordHash = UserServices.GetMd5Hash(newPassword);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index", "UserAddresss");
            }
        }


    }
}
