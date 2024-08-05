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
                    query = "SELECT * FROM Organizator WHERE orgMail = @Email AND orgPassword = @Password";
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
                                return RedirectToAction("Anasayfa", "Home");
                            }
                            else if (model.Role == "Organizator")
                            {
                                return RedirectToAction("Dashboard", "Organizator");
                            }
                            else if (model.Role == "Admin")
                            {
                                return RedirectToAction("Index", "Admin");
                            }
                        }
                        else
                        {
                            ViewBag.mesaj = "Geçersiz email veya şifre girdiniz.";
                        }
                    }
                }
            }

            return View(model);
        }
        
        /*

        [HttpPost]
        public async Task<IActionResult> Log(MasterViewModel model)
        {
            // Ensure the model state is valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prepare the query based on the role
            string query = "";
            if (model.Login.Role == "Musteri")
            {
                query = "SELECT * FROM Musteri WHERE musteriMail = @Email AND musteriPsw = @Password";
            }
            else if (model.Login.Role == "Organizator")
            {
                query = "SELECT * FROM Organizator WHERE orgMail = @Email AND orgPassword = @Password";
            }
            else if (model.Login.Role == "Admin")
            {
                query = "SELECT * FROM Admin WHERE admin_mail = @Email AND adminPsw = @Password";
            }
            else
            {
                // Handle the case where the role is not valid
                ViewBag.mesaj = "Geçersiz rol seçimi.";
                return View(model);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@Email", model.Login.Email);
                    cmd.Parameters.AddWithValue("@Password", model.Login.Password);

                    try
                    {
                        using (SqlDataReader dreader = cmd.ExecuteReader())
                        {
                            if (dreader.Read())
                            {
                                // Get user ID from the query result
                                string userID = dreader[0].ToString();

                                // Store UserID and UserRole in session
                                HttpContext.Session.SetString("UserID", userID);
                                HttpContext.Session.SetString("UserRole", model.Login.Role);

                                // Create user claims
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, model.Login.Email),
                                    new Claim(ClaimTypes.Role, model.Login.Role)
                                };

                                // Create claims identity
                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                // Set authentication properties (persistent login)
                                var authProperties = new AuthenticationProperties { IsPersistent = true };

                                // Sign in the user
                                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                                // Redirect based on user role
                                if (model.Login.Role == "Musteri")
                                {
                                    return RedirectToAction("Anasayfa", "Home");
                                }
                                else if (model.Login.Role == "Organizator")
                                {
                                    return RedirectToAction("Dashboard", "Organizator");
                                }
                                else if (model.Login.Role == "Admin")
                                {
                                    return RedirectToAction("Index", "Admin");
                                }
                            }
                            else
                            {
                                // Handle case where login fails
                                ViewBag.mesaj = "Geçersiz email veya şifre girdiniz.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception and show a generic error message
                        // For production, avoid showing the exact error to the user
                        ViewBag.mesaj = "Bir hata oluştu. Lütfen tekrar deneyiniz.";
                        // Log the error (ex) here
                    }
                }
            }

            // Return the view with the model if login fails
            return View(model);
        }
        */

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Log", "Login");
        }


        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email)
        {
            // Şifre sıfırlama işlemi burada yapılacak
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