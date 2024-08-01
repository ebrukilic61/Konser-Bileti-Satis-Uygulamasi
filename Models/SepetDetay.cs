using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class SepetDetay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SepetDetayID { get; set; }

        [ForeignKey("Sepet")]
        public int SepetID { get; set; }

        [ForeignKey("Konser")]
        public int KonserID { get; set; }

        public int KategoriID { get; set; }
        public int Miktar { get; set; }
        public decimal Fiyat { get; set; }
        public string BiletGorselPath {  get; set; }
        public string KategoriAdi {  get; set; }
        public string KonserAdi {  get; set; }
        public string SanatciAdi {  get; set; }
        public string SanatciSoyadi {  get; set; }
        public virtual Sepet Sepet { get; set; }
        public virtual Bilet Bilet { get; set; }
    }
}
