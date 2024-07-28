using KonserBiletim.Models;
using KonserBiletim.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

                            //konser.KonserTanim = reader1.GetString(reader1.GetOrdinal("konserTanim"));
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

        private List<KonserAlani> GetKonserAlanlari()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<KonserAlani>("SELECT * FROM KonserAlani").ToList();
            }
        }

        private List<Sanatci> GetSanatcilar()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<Sanatci>("SELECT * FROM Sanatci").ToList();
            }
        }

        private List<Genre> GetGenres()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<Genre>("SELECT * FROM Genre").ToList();
            }
        }

        private List<KonserDurumu> GetKonserDurumlari()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Query<KonserDurumu>("SELECT * FROM KonserDurumu").ToList();
            }
        }

        private List<BiletKategoriViewModel> GetBiletKategorileri()
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT kategoriID, kategoriName FROM BiletKategori";
                return con.Query<BiletKategoriViewModel>(query).ToList();
            }
        }

        [HttpGet]
        public IActionResult KonserEkle()
        {
            // Get lists for dropdowns
            var model = new KonserEkleViewModel
            {
                BiletKategorileri = new List<BiletKategoriViewModel>(),
                Sanatcilar = GetSanatcilar(),
                KonserAlanlari = GetKonserAlanlari(),
                KonserDurumlari = GetKonserDurumlari(),
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
                    using (var con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        // Insert konser
                        var insertKonserQuery = @"
            INSERT INTO Konser (konserName, konserDate, konserLocId, sanatciId, konserTanim) 
            VALUES (@KonserName, @KonserDate, @KonserLocId, @SanatciId, @KonserTanim);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                        var konserId = con.Query<int>(insertKonserQuery, new
                        {
                            KonserName = model.KonserName,
                            KonserDate = model.KonserDate,
                            KonserLocId = model.KonserLocId,
                            SanatciId = model.SanatciId,
                            KonserTanim = model.KonserTanim
                        }).Single();

                        if (konserId <= 0)
                        {
                            throw new Exception("Konser ID alınamadı.");
                        }

                        // Insert konser durumu
                        var insertDurumQuery = @"
            INSERT INTO KonserDurumu (konser_id, konser_durumu, yeni_tarih) 
            VALUES (@KonserId, @KonserDurumu, @YeniTarih)";

                        var rowsAffected = con.Execute(insertDurumQuery, new
                        {
                            KonserId = konserId,
                            KonserDurumu = model.KonserDurumu,
                            YeniTarih = model.YeniTarih
                        });

                        if (rowsAffected <= 0)
                        {
                            throw new Exception("Konser durumu eklenemedi.");
                        }

                        // Insert bilet kategorileri
                        var insertBiletQuery = @"
            INSERT INTO BiletKategori (konser_ID, kategoriName, biletFiyati, kisi_sayisi) 
            VALUES (@KonserId, @KategoriName, @BiletFiyati, @KisiSayisi)";

                        foreach (var kategori in model.BiletKategorileri)
                        {
                            var biletRowsAffected = con.Execute(insertBiletQuery, new
                            {
                                KonserId = konserId,
                                KategoriName = kategori.KategoriAdi,
                                BiletFiyati = kategori.Fiyat,
                                KisiSayisi = kategori.KisiSayisi
                            });

                            if (biletRowsAffected <= 0)
                            {
                                throw new Exception($"Bilet kategorisi '{kategori.KategoriAdi}' eklenemedi.");
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Konser başarıyla eklendi!";
                    return RedirectToAction("Index");
                }

                model.Sanatcilar = GetSanatcilar();
                model.KonserAlanlari = GetKonserAlanlari();
                model.KonserDurumlari = GetKonserDurumlari();
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
