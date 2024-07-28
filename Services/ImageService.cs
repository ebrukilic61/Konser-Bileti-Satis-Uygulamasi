using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ImageService
{
    private readonly Dictionary<int, string> _imageUrls;

    public ImageService()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "konserImages.json");
        var jsonData = File.ReadAllText(filePath);
        _imageUrls = JsonConvert.DeserializeObject<Dictionary<int, string>>(jsonData);
    }

    public string GetImageUrl(int sanatciId)
    {
        if (_imageUrls.TryGetValue(sanatciId, out var imageUrl))
        {
            return $"/images/singers/concerts/{imageUrl}";
        }
        return "/images/default.jpg"; // Varsayılan bir resim yolu
    }
}
