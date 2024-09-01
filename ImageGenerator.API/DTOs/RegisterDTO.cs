using System.ComponentModel.DataAnnotations;

namespace ImageGenerator.API.DTOs
{
    public class RegisterDTO
    {

        [Required]
        public string Name { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Password must be between 6 and 20 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
        public string Password { get; set; }


        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Location { get; set; }

       

        [Required]
        public string Birthday { get; set; }

        public IFormFile Image { get; set; }
    }

}
