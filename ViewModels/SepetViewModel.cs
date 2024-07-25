using KonserBiletim.Models;

namespace KonserBiletim.ViewModels
{
    public class SepetViewModel
    {
        public string KullaniciId { get; set; }
        public List<Bilet> Items { get; set; } = new List<Bilet>();
        //public decimal ToplamFiyat => Items.Sum(item => item.Fiyat * item.Miktar);

    }
}
