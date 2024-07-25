using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class KonserAlani
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int konserLocId { get; set; }

        [Required]
        [StringLength(250)]
        public string alanName { get; set; }

        [Required]
        [StringLength(250)]
        public string konserLoc { get; set; }

        [Required]
        public int capacity { get; set; }
        public virtual ICollection<Konser> Konser { get; set; }
    }
}
