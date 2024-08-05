using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class SepetDetay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int sepetDetayID { get; set; }

        [ForeignKey("Sepet")]
        public int sepetID { get; set; }

        [ForeignKey("Konser")]
        public int konserID { get; set; }

        public int kategoriID { get; set; }
        public int miktar { get; set; }
        public decimal fiyat { get; set; }
        public string BiletGorselPath {  get; set; }
        public string KategoriAdi {  get; set; }
        public string KonserAdi {  get; set; }
        public string SanatciAdi {  get; set; }
        public string SanatciSoyadi {  get; set; }
        public virtual Sepet Sepet { get; set; }
        public virtual Bilet Bilet { get; set; }
    }
}
