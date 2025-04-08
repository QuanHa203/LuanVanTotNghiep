namespace CarServer.Services.Media;

public class ImageCapturer
{
    public static async Task<bool> ScreenshotAsync(byte[] buffer, Guid guidCar)
    {
        try
        {
            string imgPath = Path.Combine(AppContext.BaseDirectory, "wwwroot" ,"Medias", guidCar.ToString(), "Screenshots", DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(imgPath))
            {
                Directory.CreateDirectory(imgPath);
            }

            string outputPath = Path.Combine(imgPath, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg");
            await File.WriteAllBytesAsync(outputPath, buffer);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
            return false;
        }
    }

}
