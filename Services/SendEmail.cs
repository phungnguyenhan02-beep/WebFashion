using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace LT_WebThoiTrang.Services
{
    public class SendEmail
    {
        public static async Task EmailSenderAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                // Đọc thông tin từ Web.config
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

                // Cấu hình SmtpClient
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                // Tạo email
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = body
                };
                mail.To.Add(recipientEmail);

                await smtpClient.SendMailAsync(mail);

                System.Diagnostics.Debug.WriteLine("Email đã được gửi thành công tới: " + recipientEmail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Có lỗi xảy ra khi gửi email: " + ex.Message);
            }
        }
    }
}
