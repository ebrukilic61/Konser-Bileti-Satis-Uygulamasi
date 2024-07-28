using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class Sanatci
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int sanatciID { get; set; }

        [ForeignKey("Genre")]
        public int genreId { get; set; }

        [Required]
        [StringLength(100)]
        public string sanatciName { get; set; }

        [Url]
        [StringLength(500)]
        public string profilFotoPath { get; set; }
        
        [StringLength(250)]
        public string description { get; set; }

        public virtual Genre Genre { get; set; }
        public virtual ICollection<Konser> Konserler { get; set; }
        public Sanatci()
        {
            Konserler = new List<Konser>();
        }
    }
}
