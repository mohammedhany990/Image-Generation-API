using System.ComponentModel.DataAnnotations;

namespace ImageGenerator.API.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }


        [DataType(DataType.Password)]
        [Required(ErrorMessage = "CurrentPassword is required")]
        public string CurrentPassword { get; set; }


        [DataType(DataType.Password)]
        [Required(ErrorMessage = "NewPassword is required")]
        public string NewPassword { get; set; }


        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Password doesn't match")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}
