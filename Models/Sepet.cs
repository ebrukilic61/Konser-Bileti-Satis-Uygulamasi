using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class Sepet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SepetID { get; set; }

        [ForeignKey("Musteri")]
        public int MusteriID { get; set; }

        public DateTime Tarih { get; set; } = DateTime.Now;

        public virtual Musteri Musteri { get; set; }
        public virtual ICollection<SepetDetay> SepetDetaylar { get; set; }
        public virtual ICollection<RezerveBilet> RezerveBiletler { get; set; }
    }
}
