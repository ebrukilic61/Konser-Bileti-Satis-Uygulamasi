using KonserBiletim.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KonserBiletim.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Data;
using System.Data.SqlClient;

namespace KonserBiletim.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Anasayfa()
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
