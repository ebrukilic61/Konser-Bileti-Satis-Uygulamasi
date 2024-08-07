using KonserBiletim.Models;

namespace KonserBiletim.ViewModels
{
    public class AdminViewModel
    {
        public int AdminID { get; set; }
        public int OrgID { get; set; }
        public string AdminName { get; set; }
        public string OrgName { get; set; }
        public string AdminMail { get; set; }
        public string OrgMail { get; set; }
        public string AdminPassword { get; set; }
        public string Role { get; set; }
        public string OrgRole {  get; set; }
        public string ProfilFotoPath { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public string OrgSurname {  get; set; }
        public List<Organizator> pendingOrganizators { get; set; }
    }
}
