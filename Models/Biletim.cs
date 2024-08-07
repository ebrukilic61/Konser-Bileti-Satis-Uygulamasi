namespace KonserBiletim.Models
{
    public class Biletim
    {
        public int musteriBiletID { get; set; }
        public int musteriID { get; set; }
        public int odemeID { get; set; }
        public string biletTuru { get; set; } // Örneğin: "VIP", "Standart"
        public int konserID { get; set; }
        public string biletDurumu { get; set; } // Örneğin: "Aktif", "İptal"
        public DateTime satinAlmaTarihi { get; set; }
        public int biletMiktar {  get; set; }

        // Navigation properties
        public Musteri Musteri { get; set; }
        public Odeme Odeme { get; set; }
        public Konser Konser { get; set; }

    }
}
