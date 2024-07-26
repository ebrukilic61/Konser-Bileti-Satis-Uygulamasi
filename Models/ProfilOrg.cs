using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class ProfilOrg
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userID { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string email { get; set; }

        [Phone]
        [StringLength(15)]
        public string telNo { get; set; }

        //[JsonPropertyName("profilFoto")]
        [Url]
        [StringLength(500)]
        public string profil_foto_path { get; set; }

        public virtual Organizator Organizator { get; set; }

    }
}
