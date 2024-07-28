using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System;
using System.Data.SqlClient;
using KonserBiletim.ViewModels;

namespace KonserBiletim.Controllers
{
    public class LoginController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30"; // Veritabanı bağlantı dizesi

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Log()
        {
            ViewBag.mesaj = null;
            return View();
        }

        [HttpPost]
        public ActionResult Log(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string query = "";
            SqlConnection conn = new SqlConnection(connectionString);

            if (model.Role == "Musteri")
            {
                query = "SELECT * FROM Musteri WHERE musteriMail = @Email AND musteriPsw = @Password";
            }
            else if (model.Role == "Organizator")
            {
                query = "SELECT * FROM Organizator WHERE orgMail = @Email AND orgPassword = @Password";
            }
            else if (model.Role == "Admin")
            {
                query = "SELECT * FROM Admin WHERE admin_mail = @Email AND adminPsw = @Password";
            }

            using(SqlCommand cmd = new SqlCommand(query, conn)) 
            {
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Password", model.Password);

                conn.Open();

                using(SqlDataReader dreader = cmd.ExecuteReader()) 
                {
                    if (dreader.Read())
                    {
                        string userID = dreader[0].ToString();
                        HttpContext.Session.SetString("UserID", userID);
                        HttpContext.Session.SetString("UserRole", model.Role);
                        
                        if(model.Role == "Musteri")
                        {
                            return RedirectToAction("Anasayfa", "Home");
                        }else if(model.Role == "Organizator")
                        {
                            return RedirectToAction("Dashboard", "Organizator");
                        }

                    }
                    else
                    {
                        ViewBag.mesaj = "Geçersiz email veya şifre girdiniz.";
                    }
                }
                
            }
              
            return View(model);
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string email)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            string newPassword = GenerateRandomPassword();
            bool isUpdated = false;

            try
            {
                conn.Open();

                // Müşteri şifresini güncelle
                cmd.CommandText = "UPDATE Musteri SET musteriPsw = @Password WHERE musteriMail = @Email";
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", newPassword);
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0) isUpdated = true;

                // Organizator şifresini güncelle
                if (!isUpdated)
                {
                    cmd.CommandText = "UPDATE Organizator SET orgPassword = @Password WHERE orgMail = @Email";
                    rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0) isUpdated = true;
                }

                // Admin şifresini güncelle
                if (!isUpdated)
                {
                    cmd.CommandText = "UPDATE Admin SET adminPsw = @Password WHERE admin_mail = @Email";
                    rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0) isUpdated = true;
                }

                if (isUpdated)
                {
                    SendEmail(email, newPassword);
                    ViewBag.Message = "Yeni şifreniz email adresinize gönderildi.";
                }
                else
                {
                    ViewBag.Message = "Email adresi bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Bir hata oluştu: " + ex.Message;
            }
            finally
            {
                conn.Close();
            }

            return View();
        }


        private string GenerateRandomPassword()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            int passwordLength = random.Next(9, 17);
            string password = new string(Enumerable.Repeat(chars, passwordLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return password;
        }

        private void SendEmail(string email, string newPassword) //daha tamamlamadım bu islemi
        {
            var fromAddress = new MailAddress("kilicebruu61@gmail.com", "KonserBiletim");
            var toAddress = new MailAddress(email);
            //const string fromPassword = " ";
            const string subject = "Şifre Sıfırlama";
            string body = $"Yeni şifreniz: {newPassword}";

            var smtp = new SmtpClient
            {
                Host = "smtp.example.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                //Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
