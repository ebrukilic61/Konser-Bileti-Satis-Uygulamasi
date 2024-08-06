using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Data;
using KonserBiletim.ViewModels;

namespace KonserBiletim.Controllers
{
    public class BiletController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public IActionResult Index()
        {
            return View();
        }



        public IActionResult Biletim(MasterViewModel model, int kategoriID)
        {
            if (model == null)
            {
                model = new MasterViewModel();
            }

            if (model.Biletim == null)
            {
                model.Biletim = new BiletViewModel();
            }

            string musteri_id = HttpContext.Session.GetString("UserID");
            int musteriID = Convert.ToInt32(musteri_id);

            List<BiletViewModel> biletler = new List<BiletViewModel>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"SELECT b.musteriBiletID, b.musteriID, b.biletTuru, b.biletDurumu, b.satinAlmaTarihi, bk.kategoriName, bk.biletFiyati, k.konserID, k.konserName,
                        s.sanatciName, k.konserDate
                        FROM Biletim b
                        JOIN BiletKategori bk ON b.konserID = bk.konser_ID
                        JOIN Konser k ON b.konserID = k.konserID
                        JOIN Sanatci s ON k.sanatciId = s.sanatciID
                        WHERE b.musteriID = @MusteriID
                        ORDER BY k.konserDate, k.konserID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@MusteriID", musteriID);

                    using (SqlDataReader dreader = cmd.ExecuteReader())
                    {
                        while (dreader.Read())
                        {
                            BiletViewModel bilet = new BiletViewModel
                            {
                                MusteriBiletID = (int)dreader["musteriBiletID"],
                                MusteriID = (int)dreader["musteriID"],
                                BiletTuru = dreader["biletTuru"].ToString(),
                                BiletDurumu = dreader["biletDurumu"].ToString(),
                                SatinAlmaTarihi = (DateTime)dreader["satinAlmaTarihi"],
                                KategoriAdi = dreader["kategoriName"].ToString(),
                                Fiyat = (decimal)dreader["biletFiyati"],
                                KonserID = (int)dreader["konserID"],
                                KonserAdi = dreader["konserName"].ToString(),
                                SanatciAdi = dreader["sanatciName"].ToString(),
                                KonserTarihi = (DateTime)dreader["konserDate"]
                            };

                            biletler.Add(bilet);
                        }
                    }
                }
            }

            model.Biletim.Biletler = biletler;
            return View(model);
        }

    }
}
