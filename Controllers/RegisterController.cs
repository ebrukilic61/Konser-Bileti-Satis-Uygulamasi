using System;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using KonserBiletim.ViewModels;
using System.Data;
using System.Net.Mail;
using System.Net;
using KonserBiletim.Services;

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
                string insertProfilQuery = "";

                if (model.Role == "Musteri")
                {
                    checkQuery = "SELECT musteriMail FROM Musteri WHERE musteriMail = @Email";
                    insertQuery = "INSERT INTO Musteri (musteriAdi, musteriSoyadi, musteriMail, musteriPsw, role) VALUES (@Name, @Surname, @Email, @Password, @Role)";
                    insertProfilQuery = "INSERT INTO Profil (userID, email) VALUES (@UserID, @Email)";
                }
                else if (model.Role == "Organizator")
                {
                    checkQuery = "SELECT orgMail FROM Organizator WHERE orgMail = @Email";
                    insertQuery = "INSERT INTO Organizator (orgName, orgSurname, orgMail, orgPassword, role) VALUES (@Name, @Surname, @Email, @Password, @Role)";
                    insertProfilQuery = "INSERT INTO ProfilOrg (userID, email) VALUES (@UserID, @Email)";
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
                        TempData["mail"] = "Bu email adresi adına bir hesap bulunmaktadır. Başka bir email adresi kullanınız.";
                        return RedirectToAction("Index", "Register");
                    }
                    else
                    {
                        if (model.Password == model.ConfirmPassword)
                        {
                            int userID;

                            //Yeni kullanıcı ekleme
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@Name", model.Name);
                                insertCmd.Parameters.AddWithValue("@Surname", model.Surname);
                                insertCmd.Parameters.AddWithValue("@Email", model.Email);
                                insertCmd.Parameters.AddWithValue("@Password", model.Password);
                                insertCmd.Parameters.AddWithValue("@Role", model.Role);

                                insertCmd.ExecuteNonQuery();

                            }

                            //Profil tablosuna kullanıcı bilgilerini ekleme
                            if (model.Role == "Musteri")
                            {

                                // Yeni eklenen kullanıcının ID'sini almak için
                                using (SqlCommand idCmd = new SqlCommand("SELECT musteri_id from Musteri where musteriMail = @Email", conn))
                                {
                                    idCmd.Parameters.AddWithValue("@Email", model.Email);
                                    userID = Convert.ToInt32(idCmd.ExecuteScalar());
                                }

                                using (SqlCommand cmd = new SqlCommand(insertProfilQuery, conn))
                                {
                                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                                    cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = model.Email; 

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            if (model.Role == "Organizator")
                            {

                                //Yeni eklenen kullanıcının ID'sini almak için
                                using (SqlCommand idCmd = new SqlCommand("SELECT orgID from Organizator where orgMail = @Email", conn))
                                {
                                    idCmd.Parameters.AddWithValue("@Email", model.Email);
                                    userID = Convert.ToInt32(idCmd.ExecuteScalar());
                                }

                                using (SqlCommand cmd = new SqlCommand(insertProfilQuery, conn))
                                {
                                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                                    cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = model.Email; 

                                    cmd.ExecuteNonQuery();
                                }

                                TempData["kayit"] = "Kaydınız alınmıştır. Admin Onayından sonra giriş yapabileceksiniz";
                                return RedirectToAction("Index", "Register");
                            }

                            //var confirmationCode = Guid.NewGuid().ToString();

                            //e posta gonderme islemi:
                            //_emailService.SendConfirmationEmail(model.Email, confirmationCode);
                            TempData["kayit2"] = "Kayıt işleminiz başarıyla gerçekleşti. Giriş Yapabilirsiniz.";
                            return RedirectToAction("Index", "Register");
                        }
                        else
                        {
                            TempData["msg"] = "Girdiğiniz şifreler eşleşmiyor. Şifreleri dikkatle giriniz.";
                            return RedirectToAction("Index","Register");
                        }
                    }
                }
            }
        }

    }
}