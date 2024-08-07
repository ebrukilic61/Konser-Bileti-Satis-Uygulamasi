using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using KonserBiletim.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonserBiletim.Controllers
{
    public class LoginController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        // GET: Login
        public IActionResult Log()
        {
            var masterViewModel = new MasterViewModel
            {
                SearchTerm = "" // veya mevcut bir değer
            };

            // MasterViewModel'i ViewBag veya ViewData ile view'e aktarma
            ViewBag.MasterViewModel = masterViewModel;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Log(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string query = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (model.Role == "Musteri")
                {
                    query = "SELECT * FROM Musteri WHERE musteriMail = @Email AND musteriPsw = @Password";
                }
                else if (model.Role == "Organizator")
                {
                    query = "SELECT * FROM Organizator WHERE orgMail = @Email AND orgPassword = @Password AND IsApproved = 1";
                }
                else if (model.Role == "Admin")
                {
                    query = "SELECT * FROM Admin WHERE admin_mail = @Email AND adminPsw = @Password";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Password", model.Password);

                    conn.Open();
                    using (SqlDataReader dreader = cmd.ExecuteReader())
                    {
                        if (dreader.Read())
                        {
                            string userID = dreader[0].ToString();
                            HttpContext.Session.SetString("UserID", userID);
                            HttpContext.Session.SetString("UserRole", model.Role);

                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, model.Email),
                                new Claim(ClaimTypes.Role, model.Role)
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties { IsPersistent = true };

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                            if (model.Role == "Musteri")
                            {
                                HttpContext.Session.SetString("UserLoggedIn", "true");
                                return RedirectToAction("Anasayfa", "Home");
                            }
                            else if (model.Role == "Organizator")
                            {
                                HttpContext.Session.SetString("UserLoggedIn", "true");
                                return RedirectToAction("Dashboard", "Organizator");
                            }
                            else if (model.Role == "Admin")
                            {
                                HttpContext.Session.SetString("UserLoggedIn", "true");
                                return RedirectToAction("Index", "Admin");
                            }
                        }
                        else
                        {
                            TempData["mesaj"] = "Geçersiz email veya şifre girdiniz.";
                            return RedirectToAction("Log");
                        }
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }


        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email)
        {
            return View();
        }

        private string GenerateRandomPassword()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            int passwordLength = random.Next(9, 17);
            string password = new string(Enumerable.Repeat(chars, passwordLength).Select(s => s[random.Next(s.Length)]).ToArray());
            return password;
        }

        private void SendEmail(string email, string newPassword)
        {
            // E-posta gönderme işlemi burada yapılacak
        }
    }
}