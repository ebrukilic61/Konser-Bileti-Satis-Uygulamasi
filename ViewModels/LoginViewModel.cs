using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Rol seçilmelidir.")]
        public string Role { get; set; } // Customer, Organizer, Admin rolleri
    }
}
