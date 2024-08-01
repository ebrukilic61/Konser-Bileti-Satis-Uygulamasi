using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using KonserBiletim.ViewModels;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using KonserBiletim.Models;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Globalization;

namespace KonserBiletim.Controllers
{
    public class SepetController : Controller
    {
        private readonly string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";
      
        //sepete bilet ekleme
        [HttpPost]
        public async Task<IActionResult> SepeteEkle(int konserId, int kategoriID, int biletSayisi, string kategoriAdi, string konserAdi, string sanatciAdi, int sanatciID, string fotoUrl, decimal biletFiyati)
        {
            
            var musteriID = HttpContext.Session.GetString("UserID");
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
                        //müşteriye ait sepet yoksa sepet oluşturuyorum:
                        string createSepetQuery = @"INSERT INTO Sepet (musteriID) OUTPUT INSERTED.SepetID VALUES (@MusteriID)";
                        using (SqlCommand createCmd = new SqlCommand(createSepetQuery, con))
                        {
                            createCmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                            sepetID = (int)await createCmd.ExecuteScalarAsync();
                            HttpContext.Session.SetInt32("SepetID", sepetID);
                        }
                    }
                }


                string konserQuery = @"SELECT konser_ID FROM BiletKategori WHERE kategoriID = @KategoriID";
                using (SqlCommand cmd = new SqlCommand(konserQuery, con))
                {
                    cmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                    var result = await cmd.ExecuteScalarAsync();
                    konserId = (result != null && result != DBNull.Value) ? (int)result : konserId;
                }

                string kategoriQuery = @"SELECT kategoriName FROM BiletKategori WHERE kategoriID = @KategoriID";
                using (SqlCommand cmd = new SqlCommand(kategoriQuery, con))
                {
                    cmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                    var result = await cmd.ExecuteScalarAsync();
                    kategoriAdi = (result != null && result != DBNull.Value) ? (string)result : kategoriAdi;
                }

                string konserAdiQuery = @"SELECT konserName FROM Konser WHERE konserID = @KonserID";
                using (SqlCommand cmd = new SqlCommand(konserAdiQuery, con))
                {
                    cmd.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserId;
                    var result = await cmd.ExecuteScalarAsync();
                    konserAdi = (result != null && result != DBNull.Value) ? (string)result : konserAdi;
                }
                
                string konserSanatciQuery = @"SELECT sanatciId FROM Konser WHERE konserID = @KonserID";
                using (SqlCommand cmd = new SqlCommand(konserSanatciQuery, con))
                {
                    cmd.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserId;
                    var result = await cmd.ExecuteScalarAsync();
                    sanatciID = (result != null && result != DBNull.Value) ? (int)result : sanatciID;
                }

                string sanatciAdiQuery = @"SELECT sanatciName FROM Sanatci WHERE sanatciID = @SanatciID";
                using (SqlCommand cmd = new SqlCommand(sanatciAdiQuery, con))
                {
                    cmd.Parameters.Add("@SanatciID", SqlDbType.Int).Value = sanatciID;
                    var result = await cmd.ExecuteScalarAsync();
                    sanatciAdi = (result != null && result != DBNull.Value) ? (string)result : sanatciAdi;
                }

                string sanatciFotoQuery = @"SELECT profilFotoPath FROM Sanatci WHERE sanatciID = @SanatciID";
                using (SqlCommand cmd = new SqlCommand(sanatciFotoQuery, con))
                {
                    cmd.Parameters.Add("@SanatciID", SqlDbType.Int).Value = sanatciID;
                    var result = await cmd.ExecuteScalarAsync();
                    fotoUrl = (result != null && result != DBNull.Value) ? (string)result : fotoUrl;
                }

                //bileti sepete ekleme islemleri:
                string checkSepetDetayQuery = @"SELECT miktar FROM SepetDetay WHERE sepetID = @SepetID AND kategoriID = @KategoriID";

                using (SqlCommand checkCmd = new SqlCommand(checkSepetDetayQuery, con))
                {
                    checkCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    checkCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                    var result = await checkCmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        //sepet detayını güncelle:
                        int mevcutMiktar = (int)result;
                        string updateSepetDetayQuery = @"UPDATE SepetDetay SET miktar = @Miktar, konserID = @KonserID, BiletGorselPath = @BiletGorselPath
                            , KategoriAdi = @KategoriAdi, KonserAdi = @KonserAdi, SanatciAdi = @SanatciAdi WHERE sepetID = @SepetID AND kategoriID = @KategoriID";

                        using (SqlCommand updateCmd = new SqlCommand(updateSepetDetayQuery, con))
                        {
                            updateCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                            updateCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                            updateCmd.Parameters.Add("@KonserID",SqlDbType.Int).Value = konserId; //YENİ EKLEDİM
                            updateCmd.Parameters.Add("@BiletGorselPath",SqlDbType.VarChar).Value = fotoUrl; //YENİ EKLEDİM
                            updateCmd.Parameters.Add("@KategoriAdi", SqlDbType.VarChar).Value = kategoriAdi; //YENİ EKLEDİM
                            updateCmd.Parameters.Add("@KonserAdi", SqlDbType.VarChar).Value = konserAdi; //YENİ EKLEDİM
                            updateCmd.Parameters.Add("@SanatciAdi", SqlDbType.VarChar).Value = sanatciAdi; //YENİ EKLEDİM
                            updateCmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = mevcutMiktar + biletSayisi;
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        //bilet fiyatı:
                        string getFiyatQuery = @"SELECT biletFiyati FROM BiletKategori WHERE kategoriID = @KategoriID";

                        using (SqlCommand getFiyatCmd = new SqlCommand(getFiyatQuery, con))
                        {
                            getFiyatCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;

                            var res = await getFiyatCmd.ExecuteScalarAsync();

                            if (await getFiyatCmd.ExecuteScalarAsync() != null && await getFiyatCmd.ExecuteScalarAsync() != DBNull.Value)
                            {
                                biletFiyati = (decimal)await getFiyatCmd.ExecuteScalarAsync();
                            }
                            else
                            {
                                //bilet fiyatı sorunu icin
                                return NotFound("Bilet fiyatı bulunamadı.");
                            }
                        }

                        string insertSepetDetayQuery = @"
                        INSERT INTO SepetDetay (sepetID, kategoriID, miktar, fiyat, konserID, BiletGorselPath, KategoriAdi, KonserAdi, SanatciAdi)
                        VALUES (@SepetID, @KategoriID, @Miktar, @BiletFiyati, @KonserID, @BiletGorselPath, @KategoriAdi, @KonserAdi, @SanatciAdi)";

                        using (SqlCommand insertCmd = new SqlCommand(insertSepetDetayQuery, con))
                        {
                            insertCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                            insertCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                            insertCmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = biletSayisi;
                            insertCmd.Parameters.Add("@BiletFiyati", SqlDbType.Decimal).Value = biletFiyati;
                            insertCmd.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserId;
                            insertCmd.Parameters.Add("@BiletGorselPath", SqlDbType.VarChar).Value = fotoUrl; //YENİ EKLEDİM
                            insertCmd.Parameters.Add("@KategoriAdi", SqlDbType.VarChar).Value = kategoriAdi; //YENİ EKLEDİM
                            insertCmd.Parameters.Add("@KonserAdi", SqlDbType.VarChar).Value = konserAdi; //YENİ EKLEDİM
                            insertCmd.Parameters.Add("@SanatciAdi", SqlDbType.VarChar).Value = sanatciAdi; //YENİ EKLEDİM

                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            return RedirectToAction("SepetGoruntule", new { sepetID = sepetID });
        }

        // Sepeti görüntüle
        [HttpGet]
        public async Task<IActionResult> SepetGoruntule()
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            if (sepetID == null)
            {
                return NotFound("Sepet bulunamadı.");
            }

            SepetViewModel model = new SepetViewModel();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Sepet detaylarını al
                string getSepetQuery = @"SELECT * FROM Sepet WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(getSepetQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID.Value;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    if (reader.Read())
                    {
                        model.SepetID = reader["sepetID"] != DBNull.Value ? (int)reader["sepetID"] : 0;
                        model.MusteriID = reader["musteriID"] != DBNull.Value ? (int)reader["musteriID"] : 0;
                    }
                    reader.Close();
                }

                // Sepet detaylarını al
                string getSepetDetayQuery = @"SELECT * FROM SepetDetay WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(getSepetDetayQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    model.SepetDetaylar = new List<SepetDetay>();
                    while (reader.Read())
                    {
                        model.SepetDetaylar.Add(new SepetDetay
                        {
                            KonserID = reader["konserID"] != DBNull.Value ? (int)reader["konserID"] : 0,
                            KategoriID = reader["kategoriID"] != DBNull.Value ? (int)reader["kategoriID"] : 0,
                            Miktar = reader["miktar"] != DBNull.Value ? (int)reader["miktar"] : 0,
                            Fiyat = reader["fiyat"] != DBNull.Value ? (decimal)reader["fiyat"] : 0,
                            KonserAdi = reader["KonserAdi"] != DBNull.Value ? (string)reader["KonserAdi"]:" ",
                            KategoriAdi = reader["KategoriAdi"] != DBNull.Value ? (string)reader["KategoriAdi"]:"",
                            SanatciAdi = reader["SanatciAdi"] != DBNull.Value ? (string)reader["SanatciAdi"]:" ",
                            BiletGorselPath = reader["BiletGorselPath"] != DBNull.Value ? (string)reader["BiletGorselPath"] :" "
                        });
                    }
                    reader.Close();

                    model.ToplamFiyat = model.SepetDetaylar.Sum(sd => sd.Fiyat * sd.Miktar);
                    model.KonserID = model.SepetDetaylar[0].KonserID;
                }

                string getNameQuery = @"
                SELECT bk.kategoriName, k.konserName, s.sanatciName, s.profilFotoPath
                FROM SepetDetay sd
                JOIN BiletKategori bk ON sd.kategoriID = bk.kategoriID
                JOIN Konser k ON sd.konserID = k.konserID
                JOIN Sanatci s ON k.sanatciID = s.sanatciID
                WHERE sd.sepetID = @SepetID";

                using (SqlCommand cmd = new SqlCommand(getNameQuery, con))
                {
                    cmd.Parameters.Add("SepetID", SqlDbType.Int).Value = model.SepetID;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    if (reader.Read())
                    {
                        model.KategoriAdi = reader["kategoriName"].ToString();
                        model.KonserAdi = reader["konserName"].ToString();
                        model.SanatciAdi = reader["sanatciName"].ToString();
                        model.BiletGorselPath = reader["profilFotoPath"]?.ToString();
                    }
                    reader.Close();
                }

            }

            if (!string.IsNullOrEmpty(model.BiletGorselPath))
            {
                model.BiletGorselPath = $"~/images/singers/icons/{model.BiletGorselPath}";
            }

            return View(model);
        }


        [HttpPost]
        public async Task<JsonResult> UpdateBiletMiktar(int konserID, int miktar)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    await con.OpenAsync();

                    var sepetID = HttpContext.Session.GetInt32("SepetID");
                    if (sepetID == null)
                    {
                        return Json(new { success = false, message = "SepetID bulunamadı." });
                    }


                    string updateQuery = @"UPDATE SepetDetay
                                   SET miktar = @Miktar
                                   WHERE konserID = @KonserID AND sepetID = @SepetID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = miktar;
                        cmd.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserID;
                        cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Güncelleme başarısız oldu." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetTotalPrice()
        {
            // Sepetteki tüm biletlerin toplam fiyatını hesapla
            var toplamFiyat = HesaplaToplamFiyat(); 

            return Json(new { success = true, totalPrice = toplamFiyat });
        }

        private decimal HesaplaToplamFiyat()
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");

            // Eğer sepetID null ise 0 döner
            if (sepetID == null)
            {
                return 0;
            }

            SepetViewModel model = GetSepetDetaylarFromDatabase(sepetID.Value);

            decimal toplamFiyat = 0;

            // SepetDetaylar null değilse ve içinde eleman varsa fiyat hesapla
            if (model.SepetDetaylar != null && model.SepetDetaylar.Any())
            {
                foreach (var item in model.SepetDetaylar)
                {
                    toplamFiyat += item.Fiyat * item.Miktar;
                }
            }

            return toplamFiyat;
        }

        private SepetViewModel GetSepetDetaylarFromDatabase(int sepetID)
        {
            SepetViewModel model = new SepetViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT sd.fiyat, sd.miktar FROM SepetDetay sd WHERE sd.SepetID = @SepetID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        model.SepetDetaylar = new List<SepetDetay>();
                        while (reader.Read())
                        {
                            model.SepetDetaylar.Add(new SepetDetay
                            {
                                Fiyat = reader.GetDecimal(reader.GetOrdinal("fiyat")),
                                Miktar = reader.GetInt32(reader.GetOrdinal("miktar"))
                            });
                        }
                        reader.Close();
                    }
                }
                return model;
            }

        }

        [HttpPost]
        public async Task<IActionResult> OdemeYap(SepetViewModel model, Dictionary<int, int> updatedQuantities)
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            if (sepetID == null)
            {
                return NotFound("Sepet bulunamadı.");
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Kart bilgilerini kontrol et
                bool kartGecerli = await KartBilgileriniDogrulaAsync(model, con);
                if (!kartGecerli)
                {
                    return BadRequest("Geçersiz kart bilgileri.");
                }

                // Sepet detaylarını güncelle
                await GuncelleSepetiAsync(sepetID.Value, updatedQuantities, con);

                // Toplam fiyatı yeniden hesapla
                decimal toplamFiyat = await HesaplaToplamFiyatAsync(sepetID.Value, con);

                // Kullanıcı puanını al ve indirim uygula
                var musteriID = HttpContext.Session.GetString("UserID");
                int kullaniciPuan = await GetKullaniciPuanAsync(musteriID, con);
                decimal indirim = HesaplaIndirim(kullaniciPuan);
                toplamFiyat -= indirim;

                // Ödeme bilgilerini ekle
                int odemeID = await EkleOdemeBilgileriniAsync(musteriID, toplamFiyat, model.KartID, con);

                // Sepeti temizle
                await SepetiTemizle(sepetID.Value, con);

                // Kullanıcı puanını güncelle
                await GuncellePuanAsync(musteriID, kullaniciPuan, con);

                // Bilet oluştur
                await OlusturBiletAsync(sepetID.Value, odemeID, con);
            }

            TempData["SuccessMessage"] = "Ödemeniz başarıyla gerçekleştirildi.";
            return RedirectToAction("Index", "Home");
        }

        private async Task<bool> KartBilgileriniDogrulaAsync(SepetViewModel model, SqlConnection con)
        {
            string checkKartQuery = @"SELECT COUNT(*) FROM KartBilgileri 
                              WHERE kart_id = @KartId 
                              AND sahip_ismi = @SahipIsmi
                              AND sahip_soyismi = @SahipSoyismi
                              AND kart_no = @KartNo 
                              AND cvv = @Cvv 
                              AND skt = @Skt
                              AND cust_id = @CustID";

            using (SqlCommand cmd = new SqlCommand(checkKartQuery, con))
            {
                cmd.Parameters.AddWithValue("@CustId", HttpContext.Session.GetString("UserID"));
                cmd.Parameters.Add("@KartId", SqlDbType.Int).Value = model.KartID;
                cmd.Parameters.Add("@SahipIsmi", SqlDbType.VarChar).Value = model.SahipIsmi;
                cmd.Parameters.Add("@SahipSoyismi", SqlDbType.VarChar).Value = model.SahipSoyismi;
                cmd.Parameters.Add("@KartNo", SqlDbType.VarChar).Value = model.KartNo;
                cmd.Parameters.Add("@Cvv", SqlDbType.Int).Value = model.CVV;
                cmd.Parameters.Add("@Skt", SqlDbType.Date).Value = model.SKT;

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
        }

        private async Task GuncelleSepetiAsync(int sepetID, Dictionary<int, int> updatedQuantities, SqlConnection con)
        {
            foreach (var kvp in updatedQuantities)
            {
                string updateQuery = @"UPDATE SepetDetay 
                               SET miktar = @Miktar 
                               WHERE sepetID = @SepetID AND kategoriID = @KategoriID";
                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    cmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kvp.Key;
                    cmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = kvp.Value;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<decimal> HesaplaToplamFiyatAsync(int sepetID, SqlConnection con)
        {
            string query = @"SELECT SUM(sd.miktar * bk.biletFiyati) 
                     FROM SepetDetay sd
                     JOIN BiletKategori bk ON sd.kategoriID = bk.kategoriID
                     WHERE sd.sepetID = @SepetID";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                return (decimal)await cmd.ExecuteScalarAsync();
            }
        }

        private async Task<int> GetKullaniciPuanAsync(string musteriID, SqlConnection con)
        {
            string query = @"SELECT Puan FROM Musteri WHERE musteri_id = @MusteriID";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? (int)result : 0;
            }
        }

        private decimal HesaplaIndirim(int kullaniciPuan)
        {
            decimal indirim = 0;
            if (kullaniciPuan >= 100) // indirim yapılabilmesi için puan en az 100 olmalı
            {
                indirim = (kullaniciPuan / 100) * 5; // puanın yüzde 5'i kadar indirim uygulama
                kullaniciPuan = 0;
            }
            return indirim;
        }

        private async Task<int> EkleOdemeBilgileriniAsync(string musteriID, decimal toplamFiyat, int kartID, SqlConnection con)
        {
            string query = @"INSERT INTO Odeme (musteriID, rezerveBilId, toplamFiyat, odemeTarihi, odemeDurumu, kartId)
                     OUTPUT INSERTED.odemeID
                     VALUES (@MusteriID, NULL, @ToplamFiyat, @OdemeTarihi, @OdemeDurumu, @KartId)";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                cmd.Parameters.Add("@ToplamFiyat", SqlDbType.Decimal).Value = toplamFiyat;
                cmd.Parameters.Add("@OdemeTarihi", SqlDbType.DateTime).Value = DateTime.UtcNow;
                cmd.Parameters.Add("@OdemeDurumu", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@KartId", SqlDbType.Int).Value = kartID;

                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        private async Task SepetiTemizle(int sepetID, SqlConnection con)
        {
            string query = @"DELETE FROM SepetDetay WHERE sepetID = @SepetID";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        [HttpPost]
        public async Task<IActionResult> BiletSil(int kategoriID, int konserID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                await BiletiSil(kategoriID, konserID, con);
            }

            return RedirectToAction("SepetGoruntule","Sepet");
        }

        private async Task BiletiSil(int kategoriID, int konserID, SqlConnection con)
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");

            string query = @"DELETE FROM SepetDetay WHERE sepetID = @SepetID AND kategoriID = @KategoriID AND konserID = @KonserID ";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                cmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                cmd.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserID;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task GuncellePuanAsync(string musteriID, int yeniPuan, SqlConnection con)
        {
            string query = @"UPDATE Musteri SET Puan = @YeniPuan WHERE musteri_id = @MusteriID";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@YeniPuan", SqlDbType.Int).Value = yeniPuan;
                cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task OlusturBiletAsync(int sepetID, int odemeID, SqlConnection con)
        {
            string query = @"INSERT INTO Bilet (sepetID, odemeID, biletTarihi)
                     SELECT @SepetID, @OdemeID, GETDATE()";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                cmd.Parameters.Add("@OdemeID", SqlDbType.Int).Value = odemeID;
                await cmd.ExecuteNonQueryAsync();
            }
        }




        /*
        // ödeme işlemi
        [HttpPost]
        public async Task<IActionResult> OdemeYap(SepetViewModel model)
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            if (sepetID == null)
            {
                return NotFound("Sepet bulunamadı.");
            }

            // Kart bilgilerini kontrol et
            bool kartGecerli = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                string checkKartQuery = @"SELECT COUNT(*) FROM KartBilgileri 
                                  WHERE kart_id = @KartId 
                                  AND sahip_ismi = @SahipIsmi
                                  AND sahip_soyismi = @SahipSoyismi
                                  AND kart_no = @KartNo 
                                  AND cvv = @Cvv 
                                  AND skt = @Skt
                                  AND cust_id = @CustID";

                using (SqlCommand cmd = new SqlCommand(checkKartQuery, con))
                {
                    cmd.Parameters.AddWithValue("@CustId", HttpContext.Session.GetString("UserID"));
                    cmd.Parameters.Add("@KartId", SqlDbType.Int).Value = model.KartID;
                    cmd.Parameters.Add("@SahipIsmi", SqlDbType.VarChar).Value = model.SahipIsmi;
                    cmd.Parameters.Add("@SahipSoyismi", SqlDbType.VarChar).Value = model.SahipSoyismi;
                    cmd.Parameters.Add("@KartNo", SqlDbType.VarChar).Value = model.KartNo;
                    cmd.Parameters.Add("@Cvv", SqlDbType.Int).Value = model.CVV;
                    cmd.Parameters.Add("@Skt", SqlDbType.Date).Value = model.SKT;

                    int count = (int)await cmd.ExecuteScalarAsync();
                    kartGecerli = count > 0;
                }

                string getSepetQuery = @"SELECT * FROM Sepet WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(getSepetQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID.Value;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    if (reader.Read())
                    {
                        model.SepetID = reader["sepetID"] != DBNull.Value ? (int)reader["sepetID"] : 0;
                        model.MusteriID = reader["musteriID"] != DBNull.Value ? (int)reader["musteriID"] : 0;
                    }
                    reader.Close();
                }


                if (!kartGecerli)
                {
                    return BadRequest("Geçersiz kart bilgileri.");
                }

                // Sepet detaylarını al
                string getSepetDetayQuery = @"SELECT * FROM SepetDetay WHERE sepetID = @SepetID";
                var sepetDetaylar = new List<SepetDetay>();

                using (SqlCommand cmd = new SqlCommand(getSepetDetayQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sepetDetaylar.Add(new SepetDetay
                            {
                                KategoriID = reader.GetInt32(reader.GetOrdinal("kategoriID")),
                                Miktar = reader.GetInt32(reader.GetOrdinal("miktar")),
                                Fiyat = reader.GetDecimal(reader.GetOrdinal("fiyat"))
                            });
                        }
                    }
                }

                // Kullanıcı bilgileri
                var musteriID = HttpContext.Session.GetString("UserID");
                string getPuanQuery = @"SELECT Puan FROM Musteri WHERE musteri_id = @MusteriID";
                int kullaniciPuan = 0;

                using (SqlCommand cmd = new SqlCommand(getPuanQuery, con))
                {
                    cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        kullaniciPuan = (int)result;
                    }
                }

                // Toplam bilet fiyatını hesapla
                decimal toplamFiyat = sepetDetaylar.Sum(sd => sd.Fiyat * sd.Miktar);

                // İndirim
                decimal indirim = 0;
                if (kullaniciPuan >= 100) // indirim yapılabilmesi için puan en az 100 olmalı
                {
                    indirim = (kullaniciPuan / 100) * 5; // puanın yüzde 5'i kadar indirim uygulama
                }

                // Toplam fiyat - indirim
                toplamFiyat = toplamFiyat - indirim;

                // Ödeme bilgilerini ekle
                string insertOdemeQuery = @"INSERT INTO Odeme (musteriID, rezerveBilId, toplamFiyat, odemeTarihi, odemeDurumu, kartId)
                                    VALUES (@MusteriID, @RezerveBilId, @ToplamFiyat, @OdemeTarihi, @OdemeDurumu, @KartId)";

                using (SqlCommand cmd = new SqlCommand(insertOdemeQuery, con))
                {
                    cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                    cmd.Parameters.Add("@RezerveBilId", SqlDbType.Int).Value = null; // Sepet kullanıldığı için bu alan NULL
                    cmd.Parameters.Add("@ToplamFiyat", SqlDbType.Decimal).Value = toplamFiyat;
                    cmd.Parameters.Add("@OdemeTarihi", SqlDbType.Timestamp).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@OdemeDurumu", SqlDbType.Bit).Value = true; // Ödeme başarılı
                    cmd.Parameters.Add("@KartId", SqlDbType.Int).Value = model.KartID;

                    await cmd.ExecuteNonQueryAsync();
                }

                // Sepeti ve detaylarını temizle
                string deleteSepetDetayQuery = @"DELETE FROM SepetDetay WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(deleteSepetDetayQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    await cmd.ExecuteNonQueryAsync();
                }

                string deleteSepetQuery = @"DELETE FROM Sepet WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(deleteSepetQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    await cmd.ExecuteNonQueryAsync();
                }

                // Puan güncelleme
                string updatePuanQuery = @"UPDATE Musteri SET Puan = Puan - @PuanKazanci WHERE musteri_id = @MusteriID";
                using (SqlCommand cmd = new SqlCommand(updatePuanQuery, con))
                {
                    cmd.Parameters.Add("@PuanKazanci", SqlDbType.Int).Value = kullaniciPuan >= 100 ? 100 : 0; // Puan 100 veya üstüyse 100 puan düş
                    cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private async Task<bool> KartBilgileriniDogruMu(SepetViewModel odemeModel)
        {

            return true;
        }
        */


    }
}
