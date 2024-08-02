using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KonserBiletim.ViewModels
{
    public class KartViewModel
    {
        [Required]
        public int CustID {  get; set; }
        public int KartID { get; set; }
        [Required]
        public string KartNo { get; set; }

        [Range(100, 999, ErrorMessage = "CVV alanı 3 rakamdan oluşmalıdır.")]
        [Required]
        public int? CVV { get; set; }

        [Required]
        public DateTime SKT { get; set; }

        [Required]
        public string SahipIsmi { get; set; }

        [Required]
        public string SahipSoyismi { get; set; }

        public string FormattedSKT => SKT.ToString("MM/yy");
    }
}
