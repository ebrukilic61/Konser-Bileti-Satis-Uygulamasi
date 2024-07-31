using System;
using System.Collections.Generic;
using KonserBiletim.Models;

namespace KonserBiletim.ViewModels
{
    public class SepetViewModel
    {
        public int SepetID { get; set; }
        public int MusteriID { get; set; }
        public int KonserID {  get; set; }
        public string KonserDurumu {  get; set; }
        public string KategoriName {  get; set; }
        public decimal BiletFiyati { get; set; }
        public int BiletSayisi {  get; set; }
        public string KonserName {  get; set; } 
        public int RezerveBiletID {  get; set; }
        public DateTime Tarih { get; set; }
        public List<SepetDetay> SepetDetaylar { get; set; }
        public decimal ToplamFiyat { get; set; }
        public decimal BonusPuan {  get; set; }
        public int KartID {  get; set; }
        public string KartNo { get; set; }
        public int CVV { get; set; }
        public DateTime SKT { get; set; }
        public string SahipIsmi { get; set; }
        public string SahipSoyismi { get; set; }
    }
}