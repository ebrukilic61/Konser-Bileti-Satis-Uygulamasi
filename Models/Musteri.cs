using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonserBiletim.Models
{
    public class Musteri
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int musteri_id { get; set; }

        [Required]
        [StringLength(50)]
        public string musteriAdi { get; set; } // Veritabanında aynı isimde olmalı

        [Required]
        [StringLength(50)]
        public string musteriSoyadi { get; set; } 

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string musteriMail { get; set; }

        [Required]
        [StringLength(16)]
        [DataType(DataType.Password)]
        public string musteriPsw { get; set; }

        [Required]
        [StringLength(20)]
        public string role { get; set; }

        [StringLength(255)]
        public string profilFotoPath {  get; set; }
        public int puan {  get; set; }  
    }

}
