namespace KonserBiletim.Models
{
    public class Odeme
    {
        public int odeme_id { get; set; }
        public int musteriID { get; set; }
        public decimal toplamFiyat { get; set; }
        public DateTime odemeTarihi { get; set; }
        public bool? odemeDurumu { get; set; } 
        public int kartId { get; set; }

        // Navigation properties
        public Musteri Musteri { get; set; }
        public KartBilgileri KartBilgileri { get; set; }
    }
}
