using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class RezerveBilet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int rezerveBiletID { get; set; }

        [ForeignKey("Bilet")]
        public int biletID { get; set; }
        public decimal totalFiyat { get; set; }
        public decimal bonusPuan { get; set; }
        public decimal finalFiyat { get; set; }

        public virtual Bilet Bilet { get; set; }
    }
}
