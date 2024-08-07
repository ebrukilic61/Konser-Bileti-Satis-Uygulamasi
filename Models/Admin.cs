namespace KonserBiletim.Models
{
    public class Admin
    {
        public int admin_id { get; set; }
        public string adminName { get; set; }
        public string adminSurname { get; set; }
        public string admin_mail { get; set; }
        public string adminPsw { get; set; }
        public string role {  get; set; }
        public string profilFotoPath {  get; set; }
    }
}
