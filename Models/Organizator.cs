using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class Organizator
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int orgID { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string orgMail { get; set; }

        [Required]
        [StringLength(100)]
        public string orgName { get; set; }

        [Required]
        [StringLength(100)]
        public string orgSurname { get; set; }

        [Required]
        [StringLength(16)]
        [DataType(DataType.Password)]
        public string orgPassword { get; set; }

        [Required]
        [StringLength(20)]
        public string role { get; set; }
        public int? concertID { get; set; }
        public bool IsApproved { get; set; }

    }
}
