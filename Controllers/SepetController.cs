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

                //bilet sayısı kontrol islemi:
                string checkBiletMiktarQuery = @"SELECT kisi_sayisi FROM BiletKategori WHERE kategoriID = @KategoriID";
                using (SqlCommand checkBiletCmd = new SqlCommand(checkBiletMiktarQuery, con))
                {
                    checkBiletCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                    var mevcutMiktarResult = await checkBiletCmd.ExecuteScalarAsync();
                    int mevcutMiktar = (mevcutMiktarResult != null && mevcutMiktarResult != DBNull.Value) ? (int)mevcutMiktarResult : 0;

                    if (mevcutMiktar <= 0)
                    {
                        return Json(new { success = false, message = "Bilet tükendi." });
                    }
                    else if (mevcutMiktar < biletSayisi)
                    {
                        return Json(new { success = false, message = "Seçtiğiniz miktarda bilet yok." });
                    }
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
            //TempData["sepet"] = "Bilet sepete eklendi.";
            return RedirectToAction("SepetGoruntule", new { sepetID = sepetID });
            //return RedirectToAction("Details","Konser", new {konserId = konserId});
        }

        //sepeti görüntüle
        [HttpGet]
        public async Task<IActionResult> SepetGoruntule()
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            if (sepetID == null)
            {
                return NotFound("Sepet bulunamadı.");
            }

            //SepetViewModel model = new SepetViewModel();

            var model = new MasterViewModel
            {
                Sepet = new SepetViewModel()
            };

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                //sepet detaylarını al
                string getSepetQuery = @"SELECT * FROM Sepet WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(getSepetQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID.Value;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    if (reader.Read())
                    {
                        model.Sepet.SepetID = reader["sepetID"] != DBNull.Value ? (int)reader["sepetID"] : 0;
                        model.Sepet.MusteriID = reader["musteriID"] != DBNull.Value ? (int)reader["musteriID"] : 0;
                    }
                    reader.Close();
                }

                //sepet detaylarını al
                string getSepetDetayQuery = @"SELECT * FROM SepetDetay WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(getSepetDetayQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    model.Sepet.SepetDetaylar = new List<SepetDetay>();
                    while (reader.Read())
                    {
                        model.Sepet.SepetDetaylar.Add(new SepetDetay
                        {
                            konserID = reader["konserID"] != DBNull.Value ? (int)reader["konserID"] : 0,
                            kategoriID = reader["kategoriID"] != DBNull.Value ? (int)reader["kategoriID"] : 0,
                            miktar = reader["miktar"] != DBNull.Value ? (int)reader["miktar"] : 0,
                            fiyat = reader["fiyat"] != DBNull.Value ? (decimal)reader["fiyat"] : 0,
                            KonserAdi = reader["KonserAdi"] != DBNull.Value ? (string)reader["KonserAdi"] : " ",
                            KategoriAdi = reader["KategoriAdi"] != DBNull.Value ? (string)reader["KategoriAdi"] : "",
                            SanatciAdi = reader["SanatciAdi"] != DBNull.Value ? (string)reader["SanatciAdi"] : " ",
                            BiletGorselPath = reader["BiletGorselPath"] != DBNull.Value ? (string)reader["BiletGorselPath"] : " "
                        });
                    }
                    reader.Close();

                    model.Sepet.ToplamFiyat = model.Sepet.SepetDetaylar.Sum(sd => sd.fiyat * sd.miktar);

                    if (!model.Sepet.SepetDetaylar.Any())
                    {
                        model.Sepet.BosSepet = true; // Boş sepet olduğunu belirten bir bayrak ekleyin
                    }
                    else
                    {
                        model.Sepet.KonserID = model.Sepet.SepetDetaylar[0].konserID;
                    }
                }

                string getNameQuery = @"SELECT bk.kategoriName, k.konserName, s.sanatciName, s.profilFotoPath FROM SepetDetay sd
                JOIN BiletKategori bk ON sd.kategoriID = bk.kategoriID
                JOIN Konser k ON sd.konserID = k.konserID
                JOIN Sanatci s ON k.sanatciID = s.sanatciID
                WHERE sd.sepetID = @SepetID";

                using (SqlCommand cmd = new SqlCommand(getNameQuery, con))
                {
                    cmd.Parameters.Add("SepetID", SqlDbType.Int).Value = model.Sepet.SepetID;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    if (reader.Read())
                    {
                        model.Sepet.KategoriAdi = reader["kategoriName"].ToString();
                        model.Sepet.KonserAdi = reader["konserName"].ToString();
                        model.Sepet.SanatciAdi = reader["sanatciName"].ToString();
                        model.Sepet.BiletGorselPath = reader["profilFotoPath"]?.ToString();
                    }
                    reader.Close();
                }

            }

            if (!string.IsNullOrEmpty(model.Sepet.BiletGorselPath))
            {
                model.Sepet.BiletGorselPath = $"~/images/singers/icons/{model.Sepet.BiletGorselPath}";
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


                    string updateQuery = "UPDATE SepetDetay SET miktar = @Miktar WHERE konserID = @KonserID AND sepetID = @SepetID";

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

            var model = GetSepetDetaylarFromDatabase(sepetID.Value);

            decimal toplamFiyat = 0;

            // SepetDetaylar null değilse ve içinde eleman varsa fiyat hesapla
            if (model.Sepet.SepetDetaylar != null && model.Sepet.SepetDetaylar.Any())
            {
                foreach (var item in model.Sepet.SepetDetaylar)
                {
                    toplamFiyat += item.fiyat * item.miktar;
                }
            }

            return toplamFiyat;
        }

        private MasterViewModel GetSepetDetaylarFromDatabase(int sepetID)
        {
            //SepetViewModel model = new SepetViewModel();

            var model = new MasterViewModel
            {
                Sepet = new SepetViewModel()
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT sd.fiyat, sd.miktar FROM SepetDetay sd WHERE sd.SepetID = @SepetID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        model.Sepet.SepetDetaylar = new List<SepetDetay>();
                        while (reader.Read())
                        {
                            model.Sepet.SepetDetaylar.Add(new SepetDetay
                            {
                                fiyat = reader.GetDecimal(reader.GetOrdinal("fiyat")),
                                miktar = reader.GetInt32(reader.GetOrdinal("miktar"))
                            });
                        }
                        reader.Close();
                    }
                }
                return model;
            }

        }


        /* ---------------Ödeme İşlemleri------------------*/

        [HttpPost]
        public async Task<IActionResult> OdemeYap(MasterViewModel model)
        {
            string userID = HttpContext.Session.GetString("UserID");
            int user_id = Convert.ToInt32(userID);

            // Kart bilgileri kontrolü
            bool kartGecerli = await KartBilgileriniDogrulaAsync(model.Sepet, user_id);
            if (!kartGecerli)
            {
                ViewBag.mesaj = "Geçersiz kart bilgileri.";
                return View("SepetGoruntule", model); // Kart bilgileri geçersizse sepete geri dön
            }

            //toplam fiyat:
            decimal toplamFiyat = await HesaplaToplamFiyatAsync();

            //ödeme bilgisi:
            await OdemeBilgileriniEkle(user_id, toplamFiyat);

            //biletim:
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();
                await BiletOlustur(model);
            }

            //sepeti temizle:
            await SepetiTemizle();

            //islem basarılı mesajı:
            TempData["odeme"] = "Ödeme işleminiz başarıyla gerçekleştirildi.";
            return RedirectToAction("SepetGoruntule", "Sepet", model);
        }


        private async Task<bool> KartBilgileriniDogrulaAsync(SepetViewModel sepetModel, int custID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                string checkKartQuery = @"SELECT COUNT(*) FROM KartBilgileri 
                   WHERE
                   sahip_ismi = @SahipIsmi
                   AND sahip_soyismi = @SahipSoyismi
                   AND kart_no = @KartNo 
                   AND cvv = @Cvv 
                   AND skt = @Skt
                   AND cust_id = @CustID";

                using (SqlCommand cmd = new SqlCommand(checkKartQuery, con))
                {
                    cmd.Parameters.Add("@CustId", SqlDbType.Int).Value = custID;
                    cmd.Parameters.Add("@SahipIsmi", SqlDbType.VarChar).Value = sepetModel.SahipIsmi;
                    cmd.Parameters.Add("@SahipSoyismi", SqlDbType.VarChar).Value = sepetModel.SahipSoyismi;
                    cmd.Parameters.Add("@KartNo", SqlDbType.VarChar).Value = sepetModel.KartNo;
                    cmd.Parameters.Add("@Cvv", SqlDbType.Int).Value = sepetModel.CVV;
                    cmd.Parameters.Add("@Skt", SqlDbType.VarChar).Value = sepetModel.SKT;

                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        private async Task SepetiTemizle()
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"DELETE FROM SepetDetay WHERE sepetID = @SepetID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<decimal> HesaplaToplamFiyatAsync()
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            using (SqlConnection con = new SqlConnection(connectionString)) 
            {
                con.Open();

                string query = @"SELECT SUM(sd.miktar * bk.biletFiyati) FROM SepetDetay sd JOIN BiletKategori bk ON sd.kategoriID = bk.kategoriID
                WHERE sd.sepetID = @SepetID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    return (decimal)await cmd.ExecuteScalarAsync();
                }
            }
        }

        private async Task BiletOlustur(MasterViewModel model)
        {
            int? sepetID = HttpContext.Session.GetInt32("SepetID");
            string userID = HttpContext.Session.GetString("UserID");

            if (!sepetID.HasValue)
            {
                throw new Exception("SepetID bulunamadı.");
            }

            if (string.IsNullOrEmpty(userID))
            {
                throw new Exception("UserID bulunamadı.");
            }

            int custID = Convert.ToInt32(userID);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                //sepetteki konserIDyi almak icin:
                int konserID;
                string selectKonserIDQuery = "SELECT DISTINCT konserID FROM SepetDetay WHERE SepetID = @SepetID";

                using (SqlCommand selectCmd = new SqlCommand(selectKonserIDQuery, con))
                {
                    selectCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;

                    var result = await selectCmd.ExecuteScalarAsync();
                    if (result == null || !int.TryParse(result.ToString(), out konserID))
                    {
                        throw new Exception("KonserID bulunamadı.");
                    }
                }

                //biletim:
                string insertQuery = @"INSERT INTO Biletim (musteriID, biletTuru, konserID, satinAlmaTarihi, biletDurumu, biletMiktar)
                               SELECT @CustID, sd.KategoriAdi, sd.konserID, GETDATE(), 'Gecerli', sd.miktar
                               FROM SepetDetay sd
                               WHERE sd.sepetID = @SepetID";

                using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.Add("@CustID", SqlDbType.Int).Value = custID;
                    insertCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;

                    await insertCmd.ExecuteNonQueryAsync();
                }

                //kalan biletlerin sayısını güncellemek icin:
                string updateBiletSayisiQuery = @"UPDATE bk
                                          SET bk.kisi_sayisi = bk.kisi_sayisi - COALESCE(sd.TotalMiktar, 0)
                                          FROM BiletKategori bk
                                          INNER JOIN (
                                              SELECT kategoriID, SUM(miktar) AS TotalMiktar
                                              FROM SepetDetay
                                              WHERE SepetID = @SepetID
                                              GROUP BY kategoriID) sd ON bk.kategoriID = sd.kategoriID
                                          WHERE bk.konser_ID = @KonserID";

                using (SqlCommand updateCmd = new SqlCommand(updateBiletSayisiQuery, con))
                {
                    updateCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    updateCmd.Parameters.Add("@KonserID", SqlDbType.Int).Value = konserID;

                    await updateCmd.ExecuteNonQueryAsync();
                }
            }
        }


        private async Task OdemeBilgileriniEkle(int musteriID, decimal toplamFiyat)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();  //kart id'yi çıkardım

                string query = @"INSERT INTO Odeme (musteriID, toplamFiyat, odemeTarihi, odemeDurumu) 
                         VALUES (@MusteriID, @ToplamFiyat, @OdemeTarihi, @OdemeDurumu)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                    cmd.Parameters.Add("@ToplamFiyat", SqlDbType.Decimal).Value = toplamFiyat;
                    cmd.Parameters.Add("@OdemeTarihi", SqlDbType.DateTime).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@OdemeDurumu", SqlDbType.Bit).Value = true;

                    await cmd.ExecuteNonQueryAsync();
                }
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
    }
}
