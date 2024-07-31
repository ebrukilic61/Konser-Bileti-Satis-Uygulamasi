using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using KonserBiletim.ViewModels;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using KonserBiletim.Models;

namespace KonserBiletim.Controllers
{
    public class SepetController : Controller
    {
        private readonly string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Lenovo\\Documents\\BiletSistemiDB.mdf;Integrated Security=True;Connect Timeout=30";

        /*
        // Sepete bilet ekleme
        [HttpPost]
        public async Task<IActionResult> SepeteEkle(int kategoriID, int miktar)
        {
            var musteriID = HttpContext.Session.GetString("UserID");
            int sepetID;
            SepetViewModel model = new SepetViewModel();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Sepeti kontrol et ve oluştur
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
                        // Yeni bir sepet oluştur
                        string createSepetQuery = @"INSERT INTO Sepet (musteriID) OUTPUT INSERTED.SepetID VALUES (@MusteriID)";
                        using (SqlCommand createCmd = new SqlCommand(createSepetQuery, con))
                        {
                            createCmd.Parameters.Add("@MusteriID", SqlDbType.Int).Value = musteriID;
                            sepetID = (int)await createCmd.ExecuteScalarAsync();
                        }
                    }
                }

                // Bilet detayını ekle
                string getFiyatQuery = @"SELECT biletFiyati FROM BiletKategori WHERE kategoriID = @KategoriID";
                decimal biletFiyati = 0;

                using (SqlCommand getFiyatCmd = new SqlCommand(getFiyatQuery, con))
                {
                    getFiyatCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                    var result = await getFiyatCmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        biletFiyati = (decimal)result;
                    }
                    else
                    {
                        // Hata yönetimi: Fiyat bilgisi bulunamadı
                        return NotFound("Bilet fiyatı bulunamadı.");
                    }
                }

                string insertSepetDetayQuery = @"
            INSERT INTO SepetDetay (sepetID, kategoriID, miktar, fiyat)
            VALUES (@SepetID, @KategoriID, @Miktar, @BiletFiyati)";

                using (SqlCommand cmd = new SqlCommand(insertSepetDetayQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    cmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                    cmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = miktar;
                    cmd.Parameters.Add("@BiletFiyati", SqlDbType.Decimal).Value = biletFiyati;

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("SepetGoruntule", new { sepetID = sepetID });
        }
        */

        // Sepete bilet ekleme
        [HttpPost]
        public async Task<IActionResult> SepeteEkle(int kategoriID, int biletSayisi)
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
                        string updateSepetDetayQuery = @"UPDATE SepetDetay SET miktar = @Miktar WHERE sepetID = @SepetID AND kategoriID = @KategoriID";

                        using (SqlCommand updateCmd = new SqlCommand(updateSepetDetayQuery, con))
                        {
                            updateCmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                            updateCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                            updateCmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = mevcutMiktar + biletSayisi;
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        //bilet fiyatı:
                        string getFiyatQuery = @"SELECT biletFiyati FROM BiletKategori WHERE kategoriID = @KategoriID";
                        decimal biletFiyati = 0;

                        using (SqlCommand getFiyatCmd = new SqlCommand(getFiyatQuery, con))
                        {
                            getFiyatCmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;

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
                        INSERT INTO SepetDetay (sepetID, kategoriID, miktar, fiyat)
                        VALUES (@SepetID, @KategoriID, @Miktar, @BiletFiyati)";

                        using (SqlCommand cmd = new SqlCommand(insertSepetDetayQuery, con))
                        {
                            cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                            cmd.Parameters.Add("@KategoriID", SqlDbType.Int).Value = kategoriID;
                            cmd.Parameters.Add("@Miktar", SqlDbType.Int).Value = biletSayisi;
                            cmd.Parameters.Add("@BiletFiyati", SqlDbType.Decimal).Value = biletFiyati;

                            await cmd.ExecuteNonQueryAsync();
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
                            Fiyat = reader["fiyat"] != DBNull.Value ? (decimal)reader["fiyat"] : 0
                        });
                    }
                    reader.Close();

                    model.ToplamFiyat = model.SepetDetaylar.Sum(sd => sd.Fiyat * sd.Miktar);
                }
            }

            return View(model);
        }


        // Ödeme işlemi
        [HttpPost]
        public async Task<IActionResult> OdemeYap(SepetViewModel model)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Sepeti ve detaylarını temizle
                string deleteSepetDetayQuery = @"DELETE FROM SepetDetay WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(deleteSepetDetayQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = model.SepetID;
                    await cmd.ExecuteNonQueryAsync();
                }

                string deleteSepetQuery = @"DELETE FROM Sepet WHERE sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(deleteSepetQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = model.SepetID;
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> BiletSil(int biletID, int sepetID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                await con.OpenAsync();

                // Seçilen biletin sepet detayından silinmesi
                string deleteBiletQuery = @"DELETE FROM SepetDetay WHERE biletID = @BiletID AND sepetID = @SepetID";
                using (SqlCommand cmd = new SqlCommand(deleteBiletQuery, con))
                {
                    cmd.Parameters.Add("@BiletID", SqlDbType.Int).Value = biletID;
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    await cmd.ExecuteNonQueryAsync();
                }

                string checkSepetQuery = @"SELECT COUNT(*) FROM SepetDetay WHERE sepetID = @SepetID"; //sepet tamamen boş mu değil mi kontrolünü yapmak için
                using (SqlCommand cmd = new SqlCommand(checkSepetQuery, con))
                {
                    cmd.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                    int itemCount = (int)await cmd.ExecuteScalarAsync();
                    if (itemCount == 0)
                    {
                        string deleteSepetQuery = @"DELETE FROM Sepet WHERE sepetID = @SepetID";
                        using (SqlCommand cmd2 = new SqlCommand(deleteSepetQuery, con))
                        {
                            cmd2.Parameters.Add("@SepetID", SqlDbType.Int).Value = sepetID;
                            await cmd2.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            return RedirectToAction("SepetGoruntule", new { sepetID = sepetID });
        }
    }
}
