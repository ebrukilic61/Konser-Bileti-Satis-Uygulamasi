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
            // M��teri ID'sini string olarak ald�n�z, bunu int'e d�n��t�rmelisiniz
            var musteriIDString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(musteriIDString) || !int.TryParse(musteriIDString, out int musteriID))
            {
                // M��teri ID'si oturumda yoksa ya da ge�ersizse hata d�nd�r
                return BadRequest("M��teri ID'si bulunamad�.");
            }

            int sepetID;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Sepet var m� kontrol et
                string checkSepetQuery = @"SELECT sepetID FROM Sepet WHERE musteriID = @MusteriID";
                using (SqlCommand cmd = new SqlCommand(checkSepetQuery, con))
                {
                    cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        sepetID = (int)result;
                    }
                    else
                    {
                        // M��teriye ait sepet yoksa sepet olu�turuyorum
                        string createSepetQuery = @"INSERT INTO Sepet (musteriID) OUTPUT INSERTED.SepetID VALUES (@MusteriID)";
                        using (SqlCommand createCmd = new SqlCommand(createSepetQuery, con))
                        {
                            createCmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                            sepetID = (int)await createCmd.ExecuteScalarAsync();
                        }
                    }

                    // Sepet ID'sini oturuma kaydet
                    HttpContext.Session.SetInt32("SepetID", sepetID);
                }
            }

            // Sepet sayfas�na y�nlendir
            return RedirectToAction("SepetGoruntule","Sepet");
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
