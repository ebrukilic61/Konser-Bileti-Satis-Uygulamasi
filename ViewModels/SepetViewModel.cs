using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KonserBiletim.Models;
using Microsoft.Build.Framework;

namespace KonserBiletim.ViewModels
{
    public class SepetViewModel
    {
        public int SepetID { get; set; }
        public int MusteriID { get; set; } //cust_id aynı zamanda
        public int KonserID {  get; set; }
        public string KonserDurumu {  get; set; }
        public decimal BiletFiyati { get; set; }
        public int BiletSayisi {  get; set; }
        public string KonserName {  get; set; } 
        public int RezerveBiletID {  get; set; }
        public DateTime Tarih { get; set; }
        public List<SepetDetay> SepetDetaylar { get; set; }
        //public List<SepetViewModel> SepetDetaylar { get; set; }
        public decimal ToplamFiyat { get; set; }
        public decimal BonusPuan {  get; set; }
        public int KartID {  get; set; }
        public string KartNo { get; set; }

        [Range(100, 999, ErrorMessage = "CVV alanı 3 rakamdan oluşmalıdır.")]
        [System.ComponentModel.DataAnnotations.Required]
        public int? CVV { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime? SKT { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string SahipIsmi { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string SahipSoyismi { get; set; }
        public int OdemeID {  get; set; }   
        public DateTime OdemeTarihi { get; set; }   //satın alma tarihi aynı zamanda
        public bool OdemeDurumu { get; set; }
        public string BiletDurumu { get; set; } //iptal edildi mi edilmedi mi
        public string KonserAdi { get; set; }
        public string SanatciAdi { get; set; }
        public string KategoriAdi { get; set; } //bilet türü aynı zamanda
        public string BiletGorselPath {  get; set; }
        public int Miktar { get; set; }
        public decimal Fiyat { get; set; }
        public bool BosSepet { get; set; } //bos sepet kontrolu icin
    }
}