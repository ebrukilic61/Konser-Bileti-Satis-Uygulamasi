using KonserBiletim.Models;

namespace KonserBiletim.ViewModels
{
    public class KonserSanatciViewModel
    {
        public IEnumerable<KonserViewModel> Konserler { get; set; }
        public IEnumerable<Sanatci> Sanatcilar { get; set; }
        public string SearchTerm { get; set; }
    }
}
