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
    }
}
