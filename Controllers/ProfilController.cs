using Microsoft.AspNetCore.Mvc;
using KonserBiletim.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Data;
using System.Globalization;
using KonserBiletim.Models;
using Dapper;

namespace KonserBiletim.Controllers
{
    public class ProfilController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public async Task<IActionResult> Profile()
        {
            string user_id = HttpContext.Session.GetString("UserID");
            int userID = Int32.Parse(user_id);
            string userRole = HttpContext.Session.GetString("UserRole");

            var model = new MasterViewModel
            {
                Profil = new ProfilViewModel
                {
                    Kartlar = new List<KartViewModel>()
                }
            };
            model.Profil.Kartlar = await GetKartlar();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "";

                if (userRole == "Musteri")
                {
                    query = "SELECT m.musteri_id, m.musteriAdi AS Name, m.musteriSoyadi AS Surname, m.musteriMail AS Email, p.telNo, p.profil_foto_path FROM Musteri m LEFT JOIN Profil p ON m.musteri_id = p.userID WHERE m.musteri_id = @UserID";
                }
                else if (userRole == "Organizator")
                {
                    query = "SELECT o.orgID, o.orgName AS Name, o.orgSurname AS Surname, o.orgMail AS Email, p.telNo, p.profil_foto_path FROM Organizator o LEFT JOIN ProfilOrg p ON o.orgID = p.userID WHERE o.orgID = @UserID";
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                    using (SqlDataReader dreader = cmd.ExecuteReader())
                    {
                        if (dreader.Read())
                        {
                            model.Profil.UserID = userID;
                            model.Profil.Name = dreader["Name"].ToString();
                            model.Profil.Surname = dreader["Surname"].ToString();
                            model.Profil.Email = dreader["Email"].ToString();
                            model.Profil.TelNo = dreader["telNo"]?.ToString();
                            model.Profil.ProfilFotoPath = dreader["profil_foto_path"]?.ToString();
                        }
                    }
                }
            }

            // Tam dosya yolunu oluşturma
            if (!string.IsNullOrEmpty(model.Profil.ProfilFotoPath))
            {
                model.Profil.ProfilFotoPath = $"~/uploads/{model.Profil.ProfilFotoPath}";
            }

            return View(model);
        }


        private async Task<IEnumerable<KartViewModel>> GetKartlar() // async olarak tanımla
        {
            List<KartViewModel> kartListesi = new List<KartViewModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string user_id = HttpContext.Session.GetString("UserID");
                int userID = Int32.Parse(user_id);

                if (userID == null)
                {
                    // Kullanıcı ID'si mevcut değilse hata verebilir veya boş liste dönebilir
                    return kartListesi;
                }

                string query = @"SELECT kart_no, cvv, skt, sahip_ismi, sahip_soyismi FROM KartBilgileri WHERE cust_id = @CustID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@CustID", SqlDbType.Int).Value = userID;
                    using (SqlDataReader dreader = await cmd.ExecuteReaderAsync())
                    {
                        while (await dreader.ReadAsync())
                        {
                            KartViewModel kart = new KartViewModel()
                            {
                                KartNo = dreader["kart_no"].ToString(),
                                CVV = Convert.ToInt32(dreader["cvv"]),
                                SKT = dreader["skt"].ToString(),
                                SahipIsmi = dreader["sahip_ismi"].ToString(),
                                SahipSoyismi = dreader["sahip_soyismi"].ToString()
                            };

                            kartListesi.Add(kart);
                        }
                    }
                }
            }
            return kartListesi;
        }

        [HttpPost]
        public async Task<IActionResult> KartEkle(string KartNo, string SahipIsmi, string SahipSoyismi, string SKT, string CVV)
        {
            string userID = HttpContext.Session.GetString("UserID");

            if (userID == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı kimliği bulunamadı.");
                return View("Profile","Profil");
            }

            if (!IsValidKartNo(KartNo)) //bunu sonradan ekledim kart numarasını kontrol edebilmek icin, ama duzgun caalismiyor
            {
                ModelState.AddModelError(string.Empty, "Kart numarası geçersiz. Kart numaranız 16 rakam içermelidir.");
                return View("Profile","Profil");
            }

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "INSERT INTO KartBilgileri (cust_id, kart_no, cvv, skt, sahip_ismi, sahip_soyismi) VALUES (@CustID, @KartNo, @CVV, @SKT, @SahipIsmi, @SahipSoyismi)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustID", int.Parse(userID));
                        command.Parameters.AddWithValue("@KartNo", KartNo);
                        command.Parameters.AddWithValue("@CVV", CVV);
                        command.Parameters.AddWithValue("@SKT", SKT);
                        command.Parameters.AddWithValue("@SahipIsmi", SahipIsmi);
                        command.Parameters.AddWithValue("@SahipSoyismi", SahipSoyismi);

                        try
                        {
                            await command.ExecuteNonQueryAsync();
                            TempData["Message"] = "Kart başarıyla eklendi!";
                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
                        }
                    }
                }
                return RedirectToAction("Profile","Profil");
            }
            return View("Profile","Profil");
        }

        private bool IsValidKartNo(string kartNo)
        {
            if (string.IsNullOrEmpty(kartNo))
            {
                return false;
            }

            string temizlenmisKartNo = kartNo.Replace(" ", "");

            return temizlenmisKartNo.Length == 16 && temizlenmisKartNo.All(char.IsDigit);
        }


        [HttpPost]
        public async Task<IActionResult> KartSil(string kart_no, int cust_id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                await KartSil(kart_no, con);
            }

            return RedirectToAction("Profile", "Profil");
        }

        private async Task KartSil(string kart_no, SqlConnection con)
        {
            string userID = HttpContext.Session.GetString("UserID");

            string query = @"DELETE FROM KartBilgileri WHERE kart_no = @KartNo AND cust_id = @CustID";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@KartNo", SqlDbType.VarChar).Value = kart_no;
                cmd.Parameters.Add("@CustID", SqlDbType.Int).Value = int.Parse(userID);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        [HttpPost]
        public ActionResult Edit(MasterViewModel model, IFormFile profilFoto)
        {
            string userID = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");

            var profilModel = model.Profil; // ProfilViewModel'i al
            var previousFotoPath = profilModel?.ProfilFotoPath; // Önceki dosya yolunu saklayın
            var fotoPath = previousFotoPath;

            if (profilFoto != null && profilFoto.Length > 0)
            {
                // Yeni dosya yüklendiğinde eski dosyayı sil
                if (!string.IsNullOrEmpty(previousFotoPath))
                {
                    DeleteFile(previousFotoPath);
                }

                // Dosya Yükleme ve Sadece Dosya Adının Dönmesi
                fotoPath = SaveUploadedFile(profilFoto);
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string updateQuery = "";

                if (userRole == "Musteri")
                {
                    updateQuery = "UPDATE Musteri SET musteriAdi = @Name, musteriSoyadi = @Surname, musteriMail = @Email WHERE musteri_id = @UserID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = profilModel?.Name ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Surname", SqlDbType.NVarChar).Value = profilModel?.Surname ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = profilModel?.Email ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = int.Parse(userID);
                        cmd.ExecuteNonQuery();
                    }

                    updateQuery = "IF EXISTS (SELECT 1 FROM Profil WHERE userID = @UserID) " +
                                  "UPDATE Profil SET telNo = @TelNo, profil_foto_path = @ProfilFotoPath WHERE userID = @UserID " +
                                  "ELSE " +
                                  "INSERT INTO Profil (userID, telNo, profil_foto_path) VALUES (@UserID, @TelNo, @ProfilFotoPath)";
                }
                else if (userRole == "Organizator")
                {
                    updateQuery = "UPDATE Organizator SET orgName = @Name, orgSurname = @Surname, orgMail = @Email WHERE orgID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = profilModel?.Name ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Surname", SqlDbType.NVarChar).Value = profilModel?.Surname ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = profilModel?.Email ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = int.Parse(userID);
                        cmd.ExecuteNonQuery();
                    }

                    updateQuery = "IF EXISTS (SELECT 1 FROM ProfilOrg WHERE userID = @UserID) " +
                                  "UPDATE ProfilOrg SET telNo = @TelNo, profil_foto_path = @ProfilFotoPath WHERE userID = @UserID " +
                                  "ELSE " +
                                  "INSERT INTO ProfilOrg (userID, telNo, profil_foto_path) VALUES (@UserID, @TelNo, @ProfilFotoPath)";
                }

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = int.Parse(userID);
                    cmd.Parameters.Add("@TelNo", SqlDbType.NVarChar).Value = (object)profilModel?.TelNo ?? DBNull.Value;
                    cmd.Parameters.Add("@ProfilFotoPath", SqlDbType.NVarChar).Value = (object)fotoPath ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(profilModel?.ProfilFotoPath))
                {
                    profilModel.ProfilFotoPath = $"~/uploads/{profilModel.ProfilFotoPath}";
                }
            }

            return RedirectToAction("Profile", "Profil");
        }


        private string SaveUploadedFile(IFormFile file)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return fileName; // Sadece dosya adını döndür
        }

        private void DeleteFile(string fileName)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var filePath = Path.Combine(uploadPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}