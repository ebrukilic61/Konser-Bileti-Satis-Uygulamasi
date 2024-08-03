using KonserBiletim.Models;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.ViewModels
{
    public class ProfilViewModel
    {
        public int UserID {  get; set; } //id
        public string Name {  get; set; }
        public string Surname {  get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string TelNo { get; set; }

        [Url]
        [StringLength(500)]
        public string ProfilFotoPath { get; set; } //profil foto url

        public IFormFile ProfilFoto { get; set; }
        public KartViewModel Kart { get; set; }
        [Required]
        public IEnumerable<KartViewModel> Kartlar { get; set; }
        //public List<KartViewModel> Karts { get; set; }
    }
}
