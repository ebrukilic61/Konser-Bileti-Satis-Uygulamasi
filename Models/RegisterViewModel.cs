using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50, ErrorMessage = "Ad çok uzun.")]
        public string Name { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Soyad çok uzun.")]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email çok uzun.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter uzunluğunda olmalıdır.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")] //compare ile sifrelerin eslesmesini kontrol ettik
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Rol çok uzun.")]
        public string Role { get; set; } // Customer, Organizer, Admin
    }

}
