using Microsoft.AspNetCore.Mvc;
using KonserBiletim.ViewModels;
using System.Data.SqlClient;

namespace KonserBiletim.Controllers
{
    public class ProfilController : Controller
    {
        private readonly string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";
        string userID; //bunu id'ye eşitlemem lazım
        public ActionResult Index()
        {
            
            return View();
        }
        public ActionResult Index(LoginViewModel model)
        {
            var modelProfil = new ProfilViewModel();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string userQuery = "";

                if(model.Role == "Musteri")
                {
                    userQuery = "SELECT musteriAdi, musteriSoyadi, musteri_id from Musteri where musteriMail = @Email";

                }else if(model.Role == "Organizator")
                {
                    userQuery = "SELECT orgName, orgSurname, orgID from Organizator where orgMail = @Email";
                }else if(model.Role == "Admin") 
                {
                    userQuery = "Select admin_id, role, admin_mail from Admin where admin_mail = @Email";
                }

                using (SqlCommand cmd = new SqlCommand(userQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    using(SqlDataReader dreader = cmd.ExecuteReader())
                    {
                        if (dreader.Read())
                        {
                            modelProfil.Name = dreader["Name"].ToString();
                            modelProfil.Email = dreader["Email"].ToString();
                           // modelProfil.User = dreader["UserID"];
                            modelProfil.TelNo = ""; // Varsayılan değer; tel no veritabanında yoksa
                            modelProfil.Avatar = ""; // Varsayılan değer; profil foto veritabanında yoksa
                        }
                    }
                }

            }
            return View(model);
        }

        public ActionResult Edit()
        {
            var model = new ProfilViewModel();
            // Bu kısımda kullanıcı bilgilerini çekip model'e atamalısınız
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ProfilViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Veritabanında güncelleme işlemi yapın
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string updateQuery = "UPDATE Profil SET telNo = @TelNo, profil_foto_path = @Avatar, email = @Email WHERE musteriMail = @Email";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@TelNo", model.TelNo);
                        cmd.Parameters.AddWithValue("@Avatar", model.Avatar);
                        cmd.Parameters.AddWithValue("@Email", model.Email);

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index", "Profil");
            }

            return View(model);
        }
    
    }

}
