namespace CarServer.Services.Media;

public class ImageCapturer
{
    public static async Task<bool> ScreenshotAsync(byte[] buffer, string imagePath)
    {
        try
        {
            int bufferLength = buffer.Length;

            if (bufferLength <= 4)
                return false;

            // Start of JPEG: 0xFF 0xD8
            if (buffer[0] != 0xFF ||
                buffer[1] != 0xD8)
                return false;

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
