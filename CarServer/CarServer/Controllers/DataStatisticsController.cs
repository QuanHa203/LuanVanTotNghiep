using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace CarServer.Controllers;

public class DataStatisticsController : Controller
{
    private readonly ILogger<DataStatisticsController> _logger;
    private readonly string _mediaPath;

    public DataStatisticsController(ILogger<DataStatisticsController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _mediaPath = Path.Combine(webHostEnvironment.WebRootPath, "Medias");
    }

    public IActionResult Index()
    {
        if (!Directory.Exists(_mediaPath))
            return View();

        Dictionary<string, List<string>> images = new();
        Dictionary<string, List<string>> videos = new();
        

        string[] guidCarPaths = Directory.GetDirectories(_mediaPath);        
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


    public IActionResult DeleteMedia([FromBody]string path)
    {
        path = Path.Combine(_mediaPath, path);
        
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            return Ok();
        }

        return BadRequest();
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