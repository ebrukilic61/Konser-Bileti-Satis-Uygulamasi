using KonserBiletim.Models;

namespace KonserBiletim.ViewModels
{
    public class KonserEkleViewModel
    {
        public string KonserName { get; set; }
        public DateTime KonserDate { get; set; }
        public int KonserLocId { get; set; }
        public int SanatciId { get; set; }
        public string SanatciName {  get; set; }
        public string KonserTanim { get; set; }
        public string KonserDurumu { get; set; }
        public DateTime? YeniTarih { get; set; }
        public List<BiletKategoriViewModel> BiletKategorileri { get; set; }
        public IEnumerable<Sanatci> Sanatcilar { get; set; }
        public IEnumerable<KonserAlani> KonserAlanlari { get; set; }
        public IEnumerable<KonserDurumu> KonserDurumlari { get; set; }
    }
}
