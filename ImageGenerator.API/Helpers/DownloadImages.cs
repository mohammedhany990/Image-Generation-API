namespace ImageGenerator.API.Helpers
{


    public class DownloadImages
    {
        private readonly HttpClient _httpClient;

        public DownloadImages()
        {
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

                // 3.Creating File Path
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
                
                Console.WriteLine($"Error downloading image: {ex.Message}");
                throw; // Rethrow the exception to handle it appropriately in the calling code
            }
        }
    }

}
