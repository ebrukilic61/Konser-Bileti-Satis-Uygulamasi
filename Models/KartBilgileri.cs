﻿namespace KonserBiletim.Models
{
    public class KartBilgileri
    {
        public int kart_id { get; set; }
        public int cust_id { get; set; }
        public string kart_no { get; set; }
        public int cvv { get; set; }
        public DateTime skt { get; set; } // Son Kullanma Tarihi
        public string sahip_ismi { get; set; }
        public string sahip_soyismi { get; set; }
        public Musteri Musteri { get; set; }
    }
}
