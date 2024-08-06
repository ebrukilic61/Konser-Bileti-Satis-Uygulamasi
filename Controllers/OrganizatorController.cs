using Dapper;
using KonserBiletim.Models;
using KonserBiletim.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace KonserBiletim.Controllers
{
    [Authorize(Roles = "Organizator")]
    public class OrganizatorController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            IEnumerable<BiletimViewModel> biletSatisVerileri;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open(); 

                string query = "SELECT satinAlmaTarihi AS Tarih, COUNT(*) AS BiletSayisi FROM Biletim GROUP BY satinAlmaTarihi ORDER BY Tarih";

                biletSatisVerileri = con.Query<BiletimViewModel>(query);

            }

            var model = new MasterViewModel
            {
                BiletSatisVerileri = biletSatisVerileri
            };

            return View(model);
        }
    }
}
