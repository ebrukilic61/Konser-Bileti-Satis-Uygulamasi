using KonserBiletim.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KonserBiletim.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Humanizer.Localisation;

namespace KonserBiletim.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        public IActionResult Index()
        {
            string userId = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");

            var sanatcilar = GetSanatcilar();
            var konserler = GetKonserler(); //varsayılan olarak tüm konserleri alıyorum

            var profilModel = new ProfilViewModel();

            var model = new MasterViewModel
            {
                KonserSanatci = new KonserSanatciViewModel
                {
                    Konserler = konserler,
                    Sanatcilar = sanatcilar
                },
                SearchTerm = null, 
                Profil = profilModel //profil modeli
            };

            if (userRole == "Organizator")
            {
                return RedirectToAction("Dashboard", "Organizator");
            }
            if(userRole == "Admin")
            {
                return RedirectToAction("Index","Admin");
            }

            return View(model); 
        }

        public async Task<IActionResult> Anasayfa(string genre = null, string searchTerm = null)
        {
            int? sepetId = HttpContext.Session.GetInt32("SepetID");
            string userId = HttpContext.Session.GetString("UserID");
            string userRole = HttpContext.Session.GetString("UserRole");

            IEnumerable<KonserViewModel> konserler;

            if (string.IsNullOrEmpty(genre))
            {
                konserler = GetKonserler();
            }
            else
            {
                konserler = GetConcertsByGenre(genre);
            }

            var sanatcilar = GetSanatcilar();

            var model = new MasterViewModel
            {
                KonserSanatci = new KonserSanatciViewModel
                {
                    Konserler = konserler,
                    Sanatcilar = sanatcilar
                },
                SearchTerm = searchTerm
            };

            await Sepet();

            if (userRole == "Organizator")
            {
                return RedirectToAction("Dashboard", "Organizator");
            }
            if (userRole == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return View(model);
        }


        private IEnumerable<KonserViewModel> GetKonserler()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
                SELECT k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                l.alanName AS KonserLoc, 
                SUM(bk.kisi_sayisi) AS Capacity,
                s.sanatciName, s.profilFotoPath AS ImageURL, 
                g.genre_name AS GenreName, d.konser_durumu AS KonserDurumu, 
                d.yeni_tarih AS YeniTarih
                FROM Konser k 
                JOIN Sanatci s ON k.sanatciId = s.sanatciID 
                JOIN KonserAlani l ON k.konserLocId = l.konserLocID
                JOIN BiletKategori bk ON k.konserID = bk.konser_ID
                JOIN Genre g ON s.genreId = g.genre_id 
                JOIN KonserDurumu d ON k.konserID = d.konser_id
                GROUP BY k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                l.alanName, s.sanatciName, s.profilFotoPath, g.genre_name, 
                d.konser_durumu, d.yeni_tarih";

                var results = con.Query<KonserViewModel>(query).ToList();

                if (results.Count == 0)
                {
                    return Enumerable.Empty<KonserViewModel>();
                }

                string fileName = "/images/singers/icons/";

                foreach (var result in results)
                {
                    result.ImageURL = fileName + result.ImageURL;

                    Console.WriteLine($"ID: {result.KonserID}, ImageURL: {result.ImageURL}");
                }

                return results;
            }
        }

        private IEnumerable<KonserViewModel> GetKonserler(string genre = null, string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
                SELECT k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                l.alanName AS KonserLoc, SUM(bk.kisi_sayisi) AS Capacity, s.sanatciName, s.profilFotoPath AS ImageURL, 
                g.genre_name AS GenreName, d.konser_durumu AS KonserDurumu, 
                d.yeni_tarih AS YeniTarih
                FROM Konser k
                JOIN Sanatci s ON k.sanatciId = s.sanatciID
                JOIN KonserAlani l ON k.konserLocId = l.konserLocID
                JOIN BiletKategori bk ON k.konserID = bk.konser_ID
                JOIN Genre g ON s.genreId = g.genre_id
                JOIN KonserDurumu d ON k.konserID = d.konser_id
                GROUP BY k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                         l.alanName, s.sanatciName, s.profilFotoPath, g.genre_name, 
                         d.konser_durumu, d.yeni_tarih
                HAVING (@genre IS NULL OR g.genre_name = @genre)
                   AND (@searchTerm IS NULL OR k.konserName LIKE '%' + @searchTerm + '%' 
                       OR k.konserTanim LIKE '%' + @searchTerm + '%')";

                var parameters = new
                {
                    genre = genre,
                    searchTerm = searchTerm
                };

                var results = con.Query<KonserViewModel>(query, parameters).ToList();

                if (results.Count == 0)
                {
                    return Enumerable.Empty<KonserViewModel>();
                }

                string fileName = "/images/singers/icons/";

                foreach (var result in results)
                {
                    result.ImageURL = fileName + result.ImageURL;
                    Console.WriteLine($"ID: {result.KonserID}, ImageURL: {result.ImageURL}");
                }

                return results;
            }
        }

        private IEnumerable<KonserViewModel> GetConcertsByGenre(string genreName)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
                SELECT k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                       l.alanName AS KonserLoc, SUM(bk.kisi_sayisi) AS Capacity, 
                       s.sanatciName, s.profilFotoPath AS ImageURL, 
                       g.genre_name AS GenreName, d.konser_durumu AS KonserDurumu, 
                       d.yeni_tarih AS YeniTarih
                FROM Konser k 
                JOIN Sanatci s ON k.sanatciId = s.sanatciID 
                JOIN KonserAlani l ON k.konserLocId = l.konserLocID 
                JOIN BiletKategori bk ON k.konserID = bk.konser_ID
                JOIN Genre g ON s.genreId = g.genre_id 
                JOIN KonserDurumu d ON k.konserID = d.konser_id
                WHERE g.genre_name = @GenreName
                GROUP BY k.konserID, k.konserName, k.konserTanim, k.konserDate, 
                         l.alanName, s.sanatciName, s.profilFotoPath, g.genre_name, 
                         d.konser_durumu, d.yeni_tarih";

                var results = con.Query<KonserViewModel>(query, new { GenreName = genreName }).ToList();

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

        public IEnumerable<KonserViewModel> GetJazzConcerts()
        {
            return GetConcertsByGenre("Jazz");
        }

        public IEnumerable<KonserViewModel> GetRockConcerts()
        {
            return GetConcertsByGenre("Rock");
        }

        public IEnumerable<KonserViewModel> GetPopConcerts()
        {
            return GetConcertsByGenre("Pop");
        }

        public IEnumerable<KonserViewModel> GetClassicalConcerts()
        {
            return GetConcertsByGenre("Klasik");
        }

        public async Task<IActionResult> Sepet()
        {
            //Müsteri ID'sini string olarak aldýnýz, bunu int'e dönüþtürmelisiniz
            var musteriIDString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(musteriIDString) || !int.TryParse(musteriIDString, out int musteriID))
            {
                //Müsteri ID'si oturumda yoksa ya da geçersizse hata döndür
                return BadRequest("Müþteri ID'si bulunamadý.");
            }

            int sepetID;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Sepet var mý kontrol et
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
                        //Müsteriye ait sepet yoksa sepet oluþturuyorum
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

            // Sepet sayfasina yönlendir
            return RedirectToAction("SepetGoruntule", "Sepet");
        }

        private IEnumerable<Sanatci> GetSanatcilar()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT sanatciID, sanatciName, profilFotoPath FROM Sanatci";

                var results = con.Query<Sanatci>(query).ToList();

                if (results.Count == 0)
                {
                    return Enumerable.Empty<Sanatci>();
                }

                // Profil fotograflarinin yolu
                string fileName = "/images/singers/icons/";

                foreach (var result in results)
                {
                    result.profilFotoPath = fileName + result.profilFotoPath;
                    Console.WriteLine($"ID: {result.sanatciID}, ProfilFotoPath: {result.profilFotoPath}");
                }

                return results;
            }
        }

        public IActionResult Iletisim()
        {

            return View();
        }

        public IActionResult About()
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