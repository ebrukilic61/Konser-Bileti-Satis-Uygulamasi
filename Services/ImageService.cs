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
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "singers", "concerts", imageUrl);
            if (File.Exists(fullPath))
            {
                return $"/images/singers/concerts/{imageUrl}";
            }
            return "/images/oneway.png";
        }
        return "/images/oneway.png";
    }
}
