using KonserBiletim.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace KonserBiletim.Controllers
{
    public class RegisterController : Controller
    {
        private readonly string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "";
                string insertQuery = "";

                if (model.Role == "Musteri")
                {
                    checkQuery = "SELECT musteriMail FROM Musteri WHERE musteriMail = @Email";
                    insertQuery = "INSERT INTO Musteri (musteriAdi, musteriSoyadi, musteriMail, musteriPsw, role) VALUES (@Name, @Surname, @Email, @Password, @Role)";
                }
                else if (model.Role == "Organizator")
                {
                    checkQuery = "SELECT orgMail FROM Organizator WHERE orgMail = @Email";
                    insertQuery = "INSERT INTO Organizator (orgName, orgSurname, orgMail, orgPassword, role) VALUES (@Name, @Surname, @Email, @Password, @Role)";
                }
                else if (model.Role == "Admin")
                {
                    checkQuery = "SELECT admin_mail FROM Admin WHERE admin_mail = @Email";
                    insertQuery = "INSERT INTO Admin (admin_mail, adminPsw, role) VALUES (@Email, @Password, @Role)";
                }

                // Mevcut kullanıcı kontrolü
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", model.Email);
                    var existingUser = checkCmd.ExecuteScalar();

                    if (existingUser != null)
                    {
                        ViewBag.Message = "Bu email adresi adına bir hesap bulunmaktadır. Başka bir email adresi kullanınız.";
                        return View(model);
                    }
                    else
                    {
                        if (model.Password == model.ConfirmPassword)
                        {
                            // Yeni kullanıcı ekleme
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@Name", model.Name);
                                insertCmd.Parameters.AddWithValue("@Surname", model.Surname);
                                insertCmd.Parameters.AddWithValue("@Email", model.Email);
                                insertCmd.Parameters.AddWithValue("@Password", model.Password);
                                insertCmd.Parameters.AddWithValue("@Role", model.Role);

                                insertCmd.ExecuteNonQuery();

                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Girdiğiniz şifreler eşleşmiyor. Şifreleri dikkatle giriniz.";
                            return View(model);
                        }
                    }
                }  
                  
            }
        }
    }
}
