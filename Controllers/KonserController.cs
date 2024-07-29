using KonserBiletim.Models;
using KonserBiletim.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace KonserBiletim.Controllers
{
    public class KonserController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        private readonly ImageService _imageService;

        public KonserController()
        {
            _imageService = new ImageService();
        }

        // Konser listesini getirir
        public IActionResult Index()
        {
            var konserler = GetKonserler();
            return View(konserler);
        }

        private IEnumerable<KonserViewModel> GetKonserler()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = @"
                   SELECT k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                   l.alanName AS KonserLoc, s.sanatciName, s.profilFotoPath AS ImageURL, 
                   g.genre_name AS GenreName, d.konser_durumu AS KonserDurumu, 
                   d.yeni_tarih AS YeniTarih, bk.biletFiyati
                   FROM Konser k 
                   JOIN Sanatci s ON k.sanatciId = s.sanatciID 
                   JOIN Genre g ON s.genreId = g.genre_id 
                   JOIN KonserAlani l ON k.konserLocId = l.konserLocID 
                   JOIN KonserDurumu d ON k.konserID = d.konser_id
                   JOIN BiletKategori bk ON k.konserID = bk.konser_ID";

                var results = con.Query<KonserViewModel>(query).ToList();

                if (results.Count == 0)
                {
                    return Enumerable.Empty<KonserViewModel>();
                }

                string fileName = "/images/singers/icons/";

                foreach (var result in results)
                {
                    result.ImageURL = fileName + result.ImageURL;
                    // Loglama
                    Console.WriteLine($"ID: {result.KonserID}, ImageURL: {result.ImageURL}");
                }

                return results;
            }
        }

        // Konser detaylarını gösterir
        public IActionResult Details(int id, int kid)
        {
            var konser = GetKonserById(id);
            if (konser == null)
            {
                return NotFound();
            }

            konser.ImageURL = _imageService.GetImageUrl(id);

            return View(konser);
        }

        public KonserViewModel GetKonserById(int konserId)
        {
            KonserViewModel konser = new KonserViewModel();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                var query1 = @"
            SELECT k.konserID, k.konserName, k.konserDate, 
                   l.alanName AS KonserLoc, s.sanatciName, s.description, s.profilFotoPath AS ImageURL, 
                   g.genre_name AS GenreName, d.konser_durumu AS KonserDurumu, 
                   d.yeni_tarih AS YeniTarih
                   FROM Konser k 
                   JOIN Sanatci s ON k.sanatciId = s.sanatciID 
                   JOIN Genre g ON s.genreId = g.genre_id 
                   JOIN KonserAlani l ON k.konserLocId = l.konserLocID 
                   JOIN KonserDurumu d ON k.konserID = d.konser_id
                   WHERE k.konserID = @KonserID";

                var query2 = @"SELECT bk.kategoriID, bk.kategoriName, bk.biletFiyati, bk.kisi_sayisi
                                FROM BiletKategori bk
                                WHERE bk.konser_ID = @KonserID";

                using (SqlCommand cmd1 = new SqlCommand(query1, con))
                {
                    cmd1.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserId;
                    using (SqlDataReader reader1 = cmd1.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            konser.KonserID = reader1.GetInt32(reader1.GetOrdinal("konserID"));
                            konser.KonserName = reader1.GetString(reader1.GetOrdinal("konserName"));
                            konser.SanatciTanim = reader1.IsDBNull(reader1.GetOrdinal("description")) ? (string?)null : reader1.GetString(reader1.GetOrdinal("description"));
                            konser.KonserDate = reader1.GetDateTime(reader1.GetOrdinal("konserDate"));
                            konser.KonserLoc = reader1.GetString(reader1.GetOrdinal("KonserLoc"));
                            konser.SanatciName = reader1.GetString(reader1.GetOrdinal("sanatciName"));
                            konser.KonserTanim = reader1.GetString(reader1.GetOrdinal("description"));
                            konser.ImageURL = reader1.GetString(reader1.GetOrdinal("ImageURL"));
                            konser.GenreName = reader1.GetString(reader1.GetOrdinal("GenreName"));
                            konser.KonserDurumu = reader1.GetString(reader1.GetOrdinal("KonserDurumu"));
                            konser.YeniTarih = reader1.IsDBNull(reader1.GetOrdinal("YeniTarih")) ? (DateTime?)null : reader1.GetDateTime(reader1.GetOrdinal("YeniTarih"));
                        }
                    }
                }

                konser.BiletKategorileri = new List<BiletKategoriViewModel>();
                using (SqlCommand cmd2 = new SqlCommand(query2, con))
                {
                    cmd2.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserId;
                    using (SqlDataReader reader2 = cmd2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            konser.BiletKategorileri.Add(new BiletKategoriViewModel
                            {
                                KategoriID = reader2.GetInt32(reader2.GetOrdinal("kategoriID")),
                                KategoriAdi = reader2.GetString(reader2.GetOrdinal("kategoriName")),
                                Fiyat = reader2.GetDecimal(reader2.GetOrdinal("biletFiyati")),
                                KisiSayisi = reader2.GetInt32(reader2.GetOrdinal("kisi_sayisi"))
                            });
                        }
                    }
                }
            }
            return konser;
        }

        [HttpGet]
        private List<KonserAlani> GetKonserAlanlari()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<KonserAlani>("SELECT * FROM KonserAlani").ToList();
            }
        }

        [HttpGet]
        private List<Sanatci> GetSanatcilar()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<Sanatci>("SELECT * FROM Sanatci").ToList();
            }
        }

        [HttpGet]
        private List<Genre> GetGenres()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<Genre>("SELECT * FROM Genre").ToList();
            }
        }

        [HttpGet]
        private List<KonserDurumu> GetKonserDurumlari()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<KonserDurumu>("SELECT * FROM KonserDurumu").ToList();
            }
        }

        [HttpGet]
        public IActionResult KonserEkle()
        {
            // Get lists for dropdowns
            var model = new KonserEkleViewModel
            {
                //BiletKategorileri = new List<KonserEkleViewModel>(),
                Sanatcilar = GetSanatcilar(),
                KonserAlanlari = GetKonserAlanlari(),
                //KonserDurumlari = GetKonserDurumlari()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult KonserEkle(KonserEkleViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        using (var transaction = con.BeginTransaction())
                        {
                            try
                            {
                                // Insert konser
                                string insertKonserQuery = @"
                                INSERT INTO Konser (konserName, konserDate, konserLocId, sanatciId, konserTanim) 
                                VALUES (@KonserName, @KonserDate, @KonserLocId, @SanatciId, @KonserTanim);
                                SELECT SCOPE_IDENTITY();";

                                int konserId;
                                using (SqlCommand cmd1 = new SqlCommand(insertKonserQuery, con, transaction))
                                {
                                    cmd1.Parameters.Add(new SqlParameter("@KonserName", model.KonserName));
                                    cmd1.Parameters.Add(new SqlParameter("@KonserDate", model.KonserDate));
                                    cmd1.Parameters.Add(new SqlParameter("@KonserLocId", model.KonserLocId));
                                    cmd1.Parameters.Add(new SqlParameter("@SanatciId", model.SanatciId));
                                    cmd1.Parameters.Add(new SqlParameter("@KonserTanim", model.KonserTanim));

                                    konserId = Convert.ToInt32(cmd1.ExecuteScalar()); //yeni eklenen konser id'si
                                }

                                // Insert konser durumu
                                string insertDurumQuery = @"
                                INSERT INTO KonserDurumu (konser_id, konser_durumu) 
                                VALUES (@KonserId, @KonserDurum)";

                                using (SqlCommand cmd2 = new SqlCommand(insertDurumQuery, con, transaction))
                                {
                                    cmd2.Parameters.AddWithValue("@KonserId", konserId);
                                    cmd2.Parameters.AddWithValue("@KonserDurum", model.KonserDurum); 

                                    cmd2.ExecuteNonQuery();
                                }

                                string insertBiletQuery = @"
                                INSERT INTO BiletKategori (konser_ID, kategoriName, biletFiyati, kisi_sayisi) 
                                VALUES (@KonserId, @KategoriAdi, @BiletFiyati, @KisiSayisi)";

                                using (SqlCommand cmd3 = new SqlCommand(insertBiletQuery, con, transaction))
                                {
                                    cmd3.Parameters.Add(new SqlParameter("@KonserId", konserId));
                                    cmd3.Parameters.Add(new SqlParameter("@KategoriAdi", model.KategoriAdi));
                                    cmd3.Parameters.Add(new SqlParameter("@BiletFiyati", model.Fiyat));
                                    cmd3.Parameters.Add(new SqlParameter("@KisiSayisi", model.KisiSayisi));
                                    cmd3.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw ex;
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Konser başarıyla eklendi!";
                    return RedirectToAction("Index");
                }
                else {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                
                }

                // Eğer model state geçerli değilse, dropdown verilerini tekrar yükle
                //model.BiletKategorileri = GetBiletKategorileri();
                model.Sanatcilar = GetSanatcilar();
                model.KonserAlanlari = GetKonserAlanlari();
                //model.KonserDurumlari = GetKonserDurumlari();
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = "Bir hata oluştu, lütfen tekrar deneyin.";
                return View(model);
            }
        }

    }
}
