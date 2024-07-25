using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class Konser
    {
        [Key]
        public int konserID {  get; set; }
       
        [Required]
        [StringLength(250)]
        public string konserName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime konserDate { get; set; } //bu veri tipi doğru mu emin değilim
        public int konserLocId {  get; set; }
        public int sanatciId {  get; set; }
        public virtual KonserAlani KonserAlani { get; set; }
        public virtual Sanatci Sanatci { get; set; }
        public virtual ICollection<Bilet> Biletler { get; set; } = new HashSet<Bilet>();
        public virtual ICollection<BiletKategori> BiletKategori { get; set; } = new HashSet<BiletKategori>();
        public virtual ICollection<KonserDurumu> KonserDurumu { get; set; } = new HashSet<KonserDurumu>();
        public virtual ICollection<Organizator> Organizatorler { get; set; } = new HashSet<Organizator>();
    }
}
