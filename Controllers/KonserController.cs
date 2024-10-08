﻿using KonserBiletim.Models;
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

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "Misafir";
        }

        //konser listesini getirir
        public IActionResult Index()
        {
            var konserler = GetKonserler(); 
            var konserSanatciViewModel = new KonserSanatciViewModel
            {
                Konserler = konserler //konserleri modele ekledim
            };

            var model = new MasterViewModel
            {
                KonserSanatci = konserSanatciViewModel,


            };

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
                    // Loglama
                    Console.WriteLine($"ID: {result.KonserID}, ImageURL: {result.ImageURL}");
                }

                return results;
            }
        }

        // Konser detaylarını gösterir

        public IActionResult Details(int id)
        {
            var konser = GetKonserById(id);
            if (konser == null)
            {
                return NotFound();
            }

            konser.Konser.ImageURL = _imageService.GetImageUrl(id);

            return View(konser);
        }

        public MasterViewModel GetKonserById(int konserId)
        {
            var masterModel = new MasterViewModel();
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

                var konser = new KonserViewModel();
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

                            if(konser.KonserDate <= DateTime.Now)
                            {
                                string updateKonserDurumQuery = @"UPDATE KonserDurumu SET konser_durumu = 'Sona Erdi' WHERE konser_id = @KonserID";

                                using(SqlCommand cmd = new SqlCommand(updateKonserDurumQuery,con)) 
                                {
                                    cmd.Parameters.AddWithValue("@KonserID", konserId);
                                    cmd.ExecuteNonQueryAsync();
                                }
                                konser.KonserDurumu = "Sona Erdi";
                            }
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
                masterModel.Konser = konser;
            }
            return masterModel;
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
            var konserEkleViewModel = new KonserEkleViewModel
            {
                Sanatcilar = GetSanatcilar(),
                KonserAlanlari = GetKonserAlanlari()
            };

            var model = new MasterViewModel
            {
                KonserEkle = konserEkleViewModel,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult KonserEkle(MasterViewModel masterModel)
        {
            var model = masterModel.KonserEkle;
            try
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
                                cmd1.Parameters.AddWithValue("@KonserName", model.KonserName);
                                cmd1.Parameters.AddWithValue("@KonserDate", model.KonserDate);
                                cmd1.Parameters.AddWithValue("@KonserLocId", model.KonserLocId);
                                cmd1.Parameters.AddWithValue("@SanatciId", model.SanatciId);
                                cmd1.Parameters.AddWithValue("@KonserTanim", model.KonserTanim);

                                konserId = Convert.ToInt32(cmd1.ExecuteScalar());
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

                            // Insert bilet kategorisi
                            foreach (var kategori in model.BiletKategorileri)
                            {
                                string insertKategoriQuery = @"
                            INSERT INTO BiletKategori (konser_ID, kategoriName, biletFiyati, kisi_sayisi) 
                            VALUES (@KonserId, @KategoriAdi, @BiletFiyati, @KisiSayisi)";

                                using (SqlCommand cmd3 = new SqlCommand(insertKategoriQuery, con, transaction))
                                {
                                    cmd3.Parameters.AddWithValue("@KonserId", konserId);
                                    cmd3.Parameters.AddWithValue("@KategoriAdi", kategori.KategoriAdi);
                                    cmd3.Parameters.AddWithValue("@BiletFiyati", kategori.Fiyat);
                                    cmd3.Parameters.AddWithValue("@KisiSayisi", kategori.KisiSayisi);

                                    cmd3.ExecuteNonQuery();
                                }
                            }


                            transaction.Commit();
                            TempData["SuccessMessage"] = "Konser başarıyla eklendi!";
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bir hata oluştu, lütfen tekrar deneyin. " + ex.Message;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult KonserSil(int konserId, int kategoriId)
        {
            if (GetUserRole() != "Organizator" && GetUserRole() != "Admin")
            {
                return Unauthorized();
            }

            MasterViewModel model = new MasterViewModel
            {
                KonserEkle = new KonserEkleViewModel
                {
                    Sanatcilar = GetSanatcilar(),
                    KonserAlanlari = GetKonserAlanlari(),
                }
            };

            using(SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Sorgular
                        string query = @"DELETE FROM Konser WHERE konserID = @KonserID";
                        string query2 = @"DELETE FROM BiletKategori WHERE kategoriID = @KategoriID AND konser_ID = @KonserID";
                        string query3 = @"DELETE FROM KonserDurumu WHERE konser_id = @KonserID";

                        // Komutları oluşturma ve parametreleri ekleme
                        using (SqlCommand command = new SqlCommand(query, con, transaction))
                        {
                            command.Parameters.AddWithValue("@KonserID", konserId);
                            command.ExecuteNonQuery();
                        }

                        using (SqlCommand command2 = new SqlCommand(query2, con, transaction))
                        {
                            command2.Parameters.AddWithValue("@KategoriID", kategoriId);
                            command2.Parameters.AddWithValue("@KonserID", konserId);
                            command2.ExecuteNonQuery();
                        }

                        using (SqlCommand command3 = new SqlCommand(query3, con, transaction))
                        {
                            command3.Parameters.AddWithValue("@KonserID", konserId);
                            command3.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            return RedirectToAction("Index","Konser");
        }


        [HttpGet]
        public IActionResult KonserDuzenle(int id)
        {
            if (GetUserRole() != "Organizator" && GetUserRole()!= "Admin") //bunu sonradan ekledim, hata yaratıyor mu diye kontrol et! -> calısıyor
            {
                return Unauthorized();
            }

            MasterViewModel model = new MasterViewModel
            {
                KonserEkle = new KonserEkleViewModel
                {
                    Sanatcilar = GetSanatcilar(),
                    KonserAlanlari = GetKonserAlanlari(),
                }
            };

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query1 = "SELECT * FROM Konser WHERE konserID = @KonserID";
                using (SqlCommand cmd1 = new SqlCommand(query1, con))
                {
                    con.Open();
                    cmd1.Parameters.Add("@KonserID", SqlDbType.Int).Value = id;
                    SqlDataReader reader1 = cmd1.ExecuteReader();

                    if (reader1.Read())
                    {
                        model.KonserEkle.KonserID = (int)reader1["konserID"];
                        model.KonserEkle.KonserName = (string)reader1["konserName"];
                        model.KonserEkle.KonserDate = (DateTime)reader1["konserDate"];
                        model.KonserEkle.KonserLocId = (int)reader1["konserLocId"];
                        model.KonserEkle.SanatciId = (int)reader1["sanatciId"];
                        model.KonserEkle.KonserTanim = reader1.IsDBNull(reader1.GetOrdinal("konserTanim")) ? string.Empty : (string)reader1["konserTanim"];
                    }
                    reader1.Close();
                }

                string query3 = @"SELECT kategoriName, biletFiyati, kisi_sayisi FROM BiletKategori WHERE konser_ID = @KonserID";
                using (SqlCommand cmd3 = new SqlCommand(query3, con))
                {
                    cmd3.Parameters.AddWithValue("@konserID", id);
                    SqlDataReader reader3 = cmd3.ExecuteReader();

                    if (reader3.Read())
                    {
                        model.KonserEkle.KategoriAdi = (string)reader3["kategoriName"];
                        model.KonserEkle.Fiyat = (decimal)reader3["biletFiyati"];
                        model.KonserEkle.KisiSayisi = (int)reader3["kisi_sayisi"];
                    }
                    reader3.Close();
                }

                string query2 = "SELECT konser_id, konser_durumu FROM KonserDurumu WHERE konser_id = @KonserID";
                using (SqlCommand cmd2 = new SqlCommand(query2, con))
                {
                    cmd2.Parameters.AddWithValue("@KonserID", id);
                    SqlDataReader reader2 = cmd2.ExecuteReader();

                    if (reader2.Read())
                    {
                        model.KonserEkle.KonserDurum = reader2.IsDBNull(reader2.GetOrdinal("konser_durumu")) ? string.Empty : (string)reader2["konser_durumu"];
                    }
                    reader2.Close();
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult KonserDuzenle(MasterViewModel model)
        {
            try
            {
                if (model.KonserEkle.KonserID == 0)
                {
                    TempData["ErrorMessage"] = "Geçersiz Konser ID.";
                    return RedirectToAction("Index");
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (var transaction = con.BeginTransaction())
                    {
                        //konser bilgilerini güncelle
                        string updateKonserQuery = @"UPDATE Konser 
                                                SET konserName = @KonserName, konserDate = @KonserDate, 
                                                    konserLocId = @KonserLocId, sanatciId = @SanatciId, 
                                                    konserTanim = @KonserTanim 
                                                WHERE konserID = @KonserID";

                        using (SqlCommand cmd = new SqlCommand(updateKonserQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@KonserName", (object)model.KonserEkle.KonserName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@KonserDate", model.KonserEkle.KonserDate);
                            cmd.Parameters.AddWithValue("@KonserLocId", model.KonserEkle.KonserLocId);
                            cmd.Parameters.AddWithValue("@SanatciId", model.KonserEkle.SanatciId);
                            cmd.Parameters.AddWithValue("@KonserTanim", (object)model.KonserEkle.KonserTanim ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@KonserID", model.KonserEkle.KonserID);

                            cmd.ExecuteNonQuery();
                        }
                        
                        //bilet Kategorileri
                        string updateBiletKategoriQuery = @"UPDATE BiletKategori
                                                    SET biletFiyati = @BiletFiyati, kisi_sayisi = @KisiSayisi
                                                    WHERE konser_ID = @KonserID AND kategoriName = @KategoriAdi";

                        using (SqlCommand cmd2 = new SqlCommand(updateBiletKategoriQuery, con, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@BiletFiyati", model.KonserEkle.Fiyat);
                            cmd2.Parameters.AddWithValue("@KisiSayisi", model.KonserEkle.KisiSayisi);
                            cmd2.Parameters.AddWithValue("@KategoriAdi", model.KonserEkle.KategoriAdi);
                            cmd2.Parameters.AddWithValue("@KonserID", model.KonserEkle.KonserID);

                            cmd2.ExecuteNonQuery();
                        }
                        

                        //KonserDurumu
                        string updateKonserDurumQuery = @"UPDATE KonserDurumu
                                                    SET konser_durumu = @KonserDurum
                                                    WHERE konser_id = @KonserID";

                        using (SqlCommand cmd3 = new SqlCommand(updateKonserDurumQuery, con, transaction))
                        {
                            cmd3.Parameters.AddWithValue("@KonserDurum", model.KonserEkle.KonserDurum);
                            cmd3.Parameters.AddWithValue("@KonserID", model.KonserEkle.KonserID);

                            cmd3.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                TempData["SuccessMessage"] = "Konser başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bir hata oluştu, lütfen tekrar deneyin.";
            }

            model.KonserEkle.Sanatcilar = GetSanatcilar();
            model.KonserEkle.KonserAlanlari = GetKonserAlanlari();
            return View(model);
        }
    }
}