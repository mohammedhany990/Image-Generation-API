namespace ImageGenerator.API.Helpers
{
    public static class DocumentSettings
    {
        public static string UploadFile(IFormFile file, string FolderName)
        {
            
            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", FolderName);

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
           
            string FileName = $"{Guid.NewGuid()}{file.FileName}";

            string FilePath = Path.Combine(FolderPath, FileName);

            using (var sf = new FileStream(FilePath, FileMode.Create))
                file.CopyTo(sf);
            return FileName;
        }

        public static bool DeleteFile(string FileName, string FolderName)
        {
            var FilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", FolderName, FileName);
            bool Ok = false;
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Ok = true;
            }
            return Ok;

        }
    }
}
