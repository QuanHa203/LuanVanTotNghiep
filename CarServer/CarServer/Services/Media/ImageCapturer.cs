namespace CarServer.Services.Media;

public class ImageCapturer
{
    public static async Task<bool> ScreenshotAsync(byte[] buffer, string imagePath)
    {
        try
        {
            imagePath = Path.Combine(imagePath, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            string outputPath = Path.Combine(imagePath, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg");
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
