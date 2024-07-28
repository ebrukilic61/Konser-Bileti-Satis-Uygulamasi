using Microsoft.AspNetCore.Mvc;
using KonserBiletim.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Data;

namespace KonserBiletim.Controllers
{
    public class ProfilController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public ActionResult Profile()
        {
            var model = new ProfilViewModel();
            string user_id = HttpContext.Session.GetString("UserID");
            int userID = Int32.Parse(user_id);
            string userRole = HttpContext.Session.GetString("UserRole");

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
                            model.UserID = userID;
                            model.Name = dreader["Name"].ToString();
                            model.Surname = dreader["Surname"].ToString();
                            model.Email = dreader["Email"].ToString();
                            model.TelNo = dreader["telNo"]?.ToString();
                            model.ProfilFotoPath = dreader["profil_foto_path"]?.ToString();
                        }
                    }
                }
            }

            // Tam dosya yolunu oluşturma
            if (!string.IsNullOrEmpty(model.ProfilFotoPath))
            {
                model.ProfilFotoPath = $"~/uploads/{model.ProfilFotoPath}";
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ProfilViewModel model, IFormFile profilFoto)
        {
            string userID = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");

            var previousFotoPath = model.ProfilFotoPath; // Önceki dosya yolunu saklayın
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
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = model.Name;
                        cmd.Parameters.Add("@Surname", SqlDbType.NVarChar).Value = model.Surname;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = int.Parse(userID);
                        cmd.ExecuteNonQuery();
                    }

                    updateQuery = "IF EXISTS (SELECT 1 FROM Profil WHERE userID = @UserID) " +
                                  "UPDATE Profil SET telNo = @TelNo, profil_foto_path = @ProfilFotoPath WHERE userID = @UserID " +
                                  "ELSE " +
                                  "INSERT INTO Profil (telNo, profil_foto_path) VALUES (@TelNo, @ProfilFotoPath)";
                }
                else if (userRole == "Organizator")
                {
                    updateQuery = "UPDATE Organizator SET orgName = @Name, orgSurname = @Surname, orgMail = @Email WHERE orgID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = model.Name;
                        cmd.Parameters.Add("@Surname", SqlDbType.NVarChar).Value = model.Surname;
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = int.Parse(userID);
                        cmd.ExecuteNonQuery();
                    }

                    updateQuery = "IF EXISTS (SELECT 1 FROM ProfilOrg WHERE userID = @UserID) " +
                                  "UPDATE ProfilOrg SET telNo = @TelNo, profil_foto_path = @ProfilFotoPath WHERE userID = @UserID " +
                                  "ELSE " +
                                  "INSERT INTO ProfilOrg (telNo, profil_foto_path) VALUES (@TelNo, @ProfilFotoPath)";
                }

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = int.Parse(userID);
                    cmd.Parameters.Add("@TelNo", SqlDbType.NVarChar).Value = (object)model.TelNo ?? DBNull.Value;
                    cmd.Parameters.Add("@ProfilFotoPath", SqlDbType.NVarChar).Value = (object)fotoPath ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrEmpty(model.ProfilFotoPath))
                {
                    model.ProfilFotoPath = $"~/uploads/{model.ProfilFotoPath}";
                }
            }
            return RedirectToAction("Profile","Profil");
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
