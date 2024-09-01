using System.ComponentModel.DataAnnotations;

namespace ImageGenerator.API.DTOs
{
    public class UpdateProfileDTO
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
