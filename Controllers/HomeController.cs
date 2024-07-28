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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Anasayfa()
        {
            /*
            var model = new ProfilViewModel();
            string user_id = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(user_id) || string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Index"); // UserID veya UserRole boþsa Index sayfasýna yönlendir
            }

            int userID;
            if (!Int32.TryParse(user_id, out userID))
            {
                return RedirectToAction("Index"); // UserID geçersizse Index sayfasýna yönlendir
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                string query = "";

                if (userRole == "Musteri")
                {
                    query = "SELECT p.profil_foto_path FROM Musteri m LEFT JOIN Profil p ON m.musteri_id = p.userID WHERE m.musteri_id = @UserID";
                }
                else if (userRole == "Organizator")
                {
                    query = "SELECT p.profil_foto_path FROM Organizator o LEFT JOIN ProfilOrg p ON o.orgID = p.userID WHERE o.orgID = @UserID";
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                    using (SqlDataReader dreader = await cmd.ExecuteReaderAsync())
                    {
                        if (await dreader.ReadAsync())
                        {
                            model.UserID = userID;
                            model.ProfilFotoPath = dreader["profil_foto_path"]?.ToString();
                        }
                    }
                }
            }

            // Tam dosya yolunu oluþturma
            if (!string.IsNullOrEmpty(model.ProfilFotoPath))
            {
                model.ProfilFotoPath = $"~/uploads/{model.ProfilFotoPath}";
            }

            ViewBag.ProfilFotoPath = model.ProfilFotoPath;
            */
            //return View(model);
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
