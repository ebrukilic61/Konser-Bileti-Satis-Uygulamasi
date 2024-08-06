namespace KonserBiletim.ViewModels
{
    public class BiletViewModel
    {
        public int KategoriID { get; set; }
        public int KonserID { get; set; }
        public string KategoriAdi { get; set; }
        public decimal Fiyat { get; set; }
        public int KisiSayisi { get; set; }
        public int MusteriBiletID { get; set; }
        public int MusteriID { get; set; }
        public string BiletTuru { get; set; }
        public string BiletDurumu {  get; set; }
        public string KonserAdi { get; set; }
        public string SanatciAdi {  get; set; }
        public IEnumerable<BiletViewModel> Biletler { get; set; }
        public DateTime SatinAlmaTarihi { get; set; }
        public DateTime KonserTarihi { get; set; }
    }
}
