using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonserBiletim.Models
{
    public class Bilet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BiletId { get; set; }

        [ForeignKey("BiletKategori")]
        public int BiletKategoriID { get; set; }
        [Required]

        [ForeignKey("Konser")]
        public int KonserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; }
        public virtual BiletKategori BiletKategori { get; set; }
        public virtual Konser Konser { get; set; }
        public virtual ICollection<Seat> Seats { get; set; } = new HashSet<Seat>();
    }
}
