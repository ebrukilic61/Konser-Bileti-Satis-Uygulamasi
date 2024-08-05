using KonserBiletim.Models;

namespace KonserBiletim.ViewModels
{
    public class MasterViewModel
    {
        public KonserSanatciViewModel KonserSanatci { get; set; }
        public SepetViewModel Sepet { get; set; }
        public ProfilViewModel Profil { get; set; }
        public KonserViewModel Konser { get; set; } 
        public KonserEkleViewModel KonserEkle { get; set; }
        public BiletKategoriViewModel Bilet {  get; set; }
        public KartViewModel Kart { get; set; }
        public string SearchTerm { get; set; }
        public KartBilgileri KartModel { get; set; }
        public Sepet SepetModel { get; set; }
        public Dictionary<int, int> UpdatedQuantities { get; set; } = new Dictionary<int, int>();
        public int SepetID { get; set; }
        public int CartItemCount { get; set; }
        public string UserID { get; set; }
    }
}
