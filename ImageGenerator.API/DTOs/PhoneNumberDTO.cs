using System.ComponentModel.DataAnnotations;

namespace ImageGenerator.API.DTOs
{
    public class PhoneNumberDTO
    {
        [Required]
        [Phone(ErrorMessage ="Please, Enter a correct Number")]
        public string PhoneNumber { get; set; }
    }
}
