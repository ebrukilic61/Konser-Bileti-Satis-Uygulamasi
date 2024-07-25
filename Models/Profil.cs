using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KonserBiletim.Models
{
    public class Profil
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
        public string profilFotoPath { get; set; }

        public virtual Musteri Musteri { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}