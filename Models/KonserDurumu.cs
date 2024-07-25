namespace KonserBiletim.Models
{
    public class KonserDurumu
    {
        public int konserStatID {  get; set; }
        public int konser_id {  get; set; }
        public string konser_durumu {  get; set; }
        public DateTime yeni_tarih { get; set; }
        public virtual Konser Konser { get; set; }
    }
}
