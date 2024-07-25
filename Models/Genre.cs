namespace KonserBiletim.Models
{
    public class Genre
    {
        public int genre_id { get; set; }
        public string genre_name { get; set; }
        public virtual ICollection<Sanatci> Sanatci { get; set; }
    }
}
