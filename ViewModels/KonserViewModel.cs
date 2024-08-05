using KonserBiletim.Models;
using System.Collections.Generic;

namespace KonserBiletim.ViewModels
{
    public class KonserViewModel
    {
        public int KonserID { get; set; }
        public int KategoriID {  get; set; }
        public string KonserName { get; set; }
        public string KonserTanim { get; set; }
        public DateTime KonserDate { get; set; }
        public int KonserLocId { get; set; }
        public int SanatciId { get; set; }
        public string SanatciName {  get; set; }
        public string SanatciTanim {  get; set; }
        public string KonserDurumu { get; set; }
        public DateTime? YeniTarih { get; set; }
        public string KonserLoc { get; set; }
        public int Capacity {  get; set; }
        public string CapacityName { get; set; }   
        public string ImageURL { get; set; }
        public string ProfilFotoPath { get; set; }
        public decimal BiletFiyati { get; set; }
        public int ToplamBiletSayisi { get; set; }
        public int GenreID {  get; set; }
        public string GenreName { get; set; }
        //public IFormFile SanatciFoto { get; set; }

        // Koleksiyonlar
        public List<BiletKategoriViewModel> BiletKategorileri { get; set; }
        public List<KonserAlani> KonserAlanlari { get; set; } = new List<KonserAlani>();
        public List<Sanatci> Sanatcilar { get; set; } = new List<Sanatci>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<KonserDurumu> KonserDurumlari { get; set; } = new List<KonserDurumu>();

    }
}
