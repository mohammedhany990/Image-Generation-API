using ImageGenerator.API.Errors;
using ImageGenerator.API.Helpers;
using ImageGenerator.Core.Entities.Data;
using ImageGenerator.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
namespace ImageGenerator.API.Controllers
{
    [Authorize]
    public class ImagesGenerationController : ApiBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly HttpClient _httpClient;
        private readonly DownloadImages _downloadImages;
        public ImagesGenerationController(UserManager<AppUser> userManager,
                                 HttpClient httpClient,
                                 DownloadImages downloadImages
                                 )
        {
            _userManager = userManager;
            _httpClient = httpClient;
            _downloadImages = downloadImages;
        }

        #region GenerateImage

        [HttpPost("GenerateImage")]
        public async Task<ActionResult> RetrieveImageUrl([FromBody] InputModel input)
        {
            if (input is null || string.IsNullOrEmpty(input.Text))
            {
                return BadRequest("Text field is required.");
            }

            var response = await _httpClient.PostAsJsonAsync("http://127.0.0.1:5000/generate", new { text = input.Text });
            response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new ApiResponse(400, "Error generating image."));
            }

            var content = await response.Content.ReadAsStringAsync();

            var imageUrl = JsonConvert.DeserializeObject<dynamic>(content).image_url;

            var email = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.FindByEmailAsync(email);

            string fileName = await _downloadImages.DownloadAndSaveImage(imageUrl.ToString(), $"{user.UserName}");
            
            user.ImagesNames.Add(fileName);
            
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                return Ok(imageUrl.ToString());
            }

            return BadRequest(new ApiResponse(400, "Failed to update user with new image."));
        }
        #endregion


        #region GetImages
        [HttpGet("GetAllImages")]
        public async Task<ActionResult> GetImages()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", user.UserName);
           
            if (!Directory.Exists(folderPath) || !Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                return NotFound(new ApiResponse(404, "Images Not Found."));
            }

            var images = Directory.GetFiles(folderPath)
                                   .Where(file => IsImageFile(file))
                                   .Select(file => new
                                   {
                                       FileName = Path.GetFileName(file),
                                       Url = Url.Action(nameof(GetImage), new { fileName = Path.GetFileName(file) })
                                   });

            return Ok(images);
        }
        #endregion


        #region GetImage

        [HttpGet("GetImage")]
        public async Task<ActionResult> GetImage(string fileName)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", user.UserName);

            var filePath = Path.Combine(folderPath, fileName);
            if (!System.IO.File.Exists(filePath) || !IsImageFile(filePath))
            {
                return NotFound(new ApiResponse(404, "Image not found."));
            }

            var image = System.IO.File.OpenRead(filePath);

            return File(image, "image/jpeg");
        }

        #endregion

        #region DeleteImage 
        [HttpDelete("DeleteImage")]
        public async Task<ActionResult> DeleteImage(string fileName)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", user.UserName);

            bool DeletedOk = DocumentSettings.DeleteFile(fileName, folderPath);


            var filePath = Path.Combine(folderPath, fileName);
            if (!DeletedOk)
            {
                return NotFound(new ApiResponse(404, "Image not found."));
            }
            return Ok(new ApiResponse(200, "Deleted Successfully!"));
        }


        #endregion

        #region DeleteAllImages 
        [HttpDelete("DeleteAllImages")]
        public async Task<ActionResult> DeleteAllImages(string fileName)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", user.UserName);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                return Ok(new ApiResponse(200 , "Deleted Successfully!"));
            }
            return BadRequest(new ApiResponse(404, "Not Found"));
        }
        #endregion


        private bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
            string fileExtension = Path.GetExtension(filePath).ToLower();
            return imageExtensions.Contains(fileExtension);
        }
    }
}
