using KonserBiletim.Models;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.ViewModels
{
    public class MasterViewModel
    {
        public KonserSanatciViewModel KonserSanatci { get; set; }
        public SepetViewModel Sepet { get; set; }
        public ProfilViewModel Profil { get; set; }
        public KonserViewModel Konser { get; set; } 
        public KonserEkleViewModel KonserEkle { get; set; }
        public IEnumerable<KonserViewModel> Konserler { get; set; }
        public BiletKategoriViewModel Bilet {  get; set; }
        public BiletViewModel Biletim {  get; set; }
        public KartViewModel Kart { get; set; }
        public string SearchTerm { get; set; }
        public KartBilgileri KartModel { get; set; }
        public Sepet SepetModel { get; set; }
        public IEnumerable<BiletimViewModel> BiletSatisVerileri { get; set; }
        public LoginViewModel Login { get; set; } = new LoginViewModel();
        public RegisterViewModel Register { get; set; } = new RegisterViewModel();
        public AdminViewModel Admin { get; set; }
    }
}
