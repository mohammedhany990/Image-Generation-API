using System.ComponentModel.DataAnnotations;

namespace ImageGenerator.API.DTOs
{
    public class UserDTO
    {
        public string Name { get; set; }
        public string ImageName { get; set; }
        public IFormFile Image { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Birthday { get; set; }


    }


}
