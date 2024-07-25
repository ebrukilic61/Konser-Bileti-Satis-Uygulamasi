namespace KonserBiletim.Models
{
    public class Seat
    {
        public int seatID { get; set; }
        public int biletID { get; set; }
        public int seatNum { get; set; }
        public virtual Bilet Bilet { get; set; }
    }
}
