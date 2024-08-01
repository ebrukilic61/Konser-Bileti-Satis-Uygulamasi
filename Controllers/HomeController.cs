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
            var model = new ProfilViewModel();
            string user_id = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Organizator")
            {
                return RedirectToAction("Dashboard", "Organizator");
            }
            return View();
        }

        public async Task<IActionResult> Anasayfa()
        {
            return View();
        }

        public async Task<IActionResult> Sepet()
        {
            // musteriID'yi string olarak aldýnýz, bunu int'e dönüþtürmelisiniz
            var musteriIDString = HttpContext.Session.GetString("UserID");
            int musteriID = int.Parse(musteriIDString);
            int sepetID;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                string checkSepetQuery = @"SELECT sepetID FROM Sepet WHERE musteriID = @MusteriID";

                using (SqlCommand cmd = new SqlCommand(checkSepetQuery, con))
                {
                    cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        sepetID = (int)result;
                        HttpContext.Session.SetInt32("SepetID", sepetID);
                    }
                    else
                    {
                        // Müþteriye ait sepet yoksa sepet oluþturuyorum
                        string createSepetQuery = @"INSERT INTO Sepet (musteriID) OUTPUT INSERTED.SepetID VALUES (@MusteriID)";
                        using (SqlCommand createCmd = new SqlCommand(createSepetQuery, con))
                        {
                            createCmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                            sepetID = (int)await createCmd.ExecuteScalarAsync();
                            HttpContext.Session.SetInt32("SepetID", sepetID);
                        }
                    }
                }
            }
            return RedirectToAction("SepetGoruntule", "Sepet");
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
