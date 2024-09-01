namespace ImageGenerator.API.Helpers
{


    public class DownloadImages
    {
        private readonly ILogger<DownloadImages> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly HttpClient _httpClient;

        public DownloadImages( ILogger<DownloadImages> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async  Task<string> DownloadAndSaveImage(string imageUrl, string savePath)
        {
            try
            {
                // Download image bytes
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);

                // Save image to file
                string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", savePath);
                
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }

                string FileName = $"{Guid.NewGuid()}.jpg";

                // Creating File Path
                string FilePath = Path.Combine(FolderPath, FileName);
               

                // Write the image bytes to the file
                File.WriteAllBytes(FilePath, imageBytes);

                /*
                using (var sf = new FileStream(FilePath, FileMode.Create))
                {
                    await sf.WriteAsync(imageBytes, 0, imageBytes.Length);
                }*/

                return $"{FileName}.jpg";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading image: {ex.Message}");
                throw; 
            }
        }
    }

}
