namespace KonserBiletim.Models
{
    public class SepetModelView
    {
        public string KullaniciId { get; set; }
        public List<Bilet> Items { get; set; } = new List<Bilet>();
        //public decimal ToplamFiyat => Items.Sum(item => item.Fiyat * item.Miktar);

    }
}
