using System.ComponentModel.DataAnnotations;

namespace ImageGenerator.API.DTOs
{
    public class UpdateBirthdayDTO
    {
        [Required]
        public string Date { get; set; }
    }
}
