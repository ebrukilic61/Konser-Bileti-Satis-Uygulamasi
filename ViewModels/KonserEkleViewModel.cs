﻿using KonserBiletim.Models;
using System.ComponentModel.DataAnnotations;

namespace KonserBiletim.ViewModels
{
    public class KonserEkleViewModel
    {
        [Required]
        public int KonserID { get; set; }

        [Required]
        [StringLength(100)]
        public string KonserName { get; set; }

        [Required]
        public DateTime KonserDate { get; set; }

        [Required]
        public int KonserLocId { get; set; }

        [Required]
        public int SanatciId { get; set; }
        //public string SanatciName { get; set; }

        [StringLength(200)]
        public string KonserTanim { get; set; }

        [Required]  
        public string KonserDurum { get; set; }

        public DateTime? YeniTarih { get; set; }
        [Required]
        public int KategoriID { get; set; }
        [Required]
        public string KategoriAdi { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat pozitif bir değer olmalıdır.")]
        public decimal Fiyat { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Kişi sayısı en az 1 olmalıdır.")]
        public int KisiSayisi { get; set; }

        public List<KonserAlani> KonserAlanlari { get; set; } = new List<KonserAlani>();
        public List<BiletKategoriViewModel> BiletKategorileri { get; set; } = new List<BiletKategoriViewModel>();
        public List<Sanatci> Sanatcilar { get; set; } = new List<Sanatci>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
        //public List<KonserDurumu> KonserDurumlari { get; set; } = new List<KonserDurumu>();
    }

}
