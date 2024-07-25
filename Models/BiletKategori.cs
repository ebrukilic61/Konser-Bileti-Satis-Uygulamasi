using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class BiletKategori
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int kategoriID {  get; set; }

        [ForeignKey("Konser")]
        public int konser_ID {  get; set; }

        [Required]
        [StringLength(100)]
        public string kategoriName {  get; set; }
        [Required]
        public double biletFiyati {  get; set; }
        [Required]
        public int kisi_sayisi {  get; set; }
        public virtual Konser Konser { get; set; }
        public virtual ICollection<Bilet> Biletler { get; set; } = new HashSet<Bilet>(); //bu kategoriye ait biletler listelensin
    }
}
