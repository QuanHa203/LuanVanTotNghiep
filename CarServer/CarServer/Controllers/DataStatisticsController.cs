using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace CarServer.Controllers;

public class DataStatisticsController : Controller
{
    private readonly ILogger<DataStatisticsController> _logger;
    private static readonly string mediaPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Medias");

    public DataStatisticsController(ILogger<DataStatisticsController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!Directory.Exists(mediaPath))
            return View();

        Dictionary<string, List<string>> images = new();
        Dictionary<string, List<string>> videos = new();
        

        string[] guidCarPaths = Directory.GetDirectories(mediaPath);        
        foreach (string guidCarPath in guidCarPaths)
        {
            string guidCarFileName = Path.GetFileName(guidCarPath);
            StringBuilder mediaUrl = new StringBuilder(Request.Scheme).Append("://").Append(Request.Host).Append("/Medias/").Append(guidCarFileName).Append('/');

            var imgList = new List<string>();
            var vidList = new List<string>();
            GetImagesAndVideoUrl(guidCarPath, mediaUrl, imgList, vidList);
            images.Add(guidCarFileName, imgList);
            videos.Add(guidCarFileName, vidList);
        }

        ViewData["DictionaryVideo"] = videos;
        ViewData["DictionaryImage"] = images;
        return View();
    }

    private static void GetImagesAndVideoUrl(string path, StringBuilder sb, List<string> imageList, List<string> videoList)
    {
        int baseLength = sb.Length;

        string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            int oldLength = sb.Length;
            string fileName = Path.GetFileName(file);
            sb.Append(fileName);

            string extension = Path.GetExtension(file);
            if (extension is ".jpg")
                imageList.Add(sb.ToString());

            else if (extension is ".mp4")
                videoList.Add(sb.ToString());

            sb.Length = baseLength;
        }
        
        string[] directories = Directory.GetDirectories(path);
        foreach (var dir in directories)
        {            
            string folderName = Path.GetFileName(dir);
            sb.Append(folderName).Append('/');
            GetImagesAndVideoUrl(dir, sb, imageList, videoList);
            sb.Length = baseLength;
        }
    }
}