using ImageGenerator.API.DTOs;
using ImageGenerator.API.Errors;
using ImageGenerator.API.Helpers;
using ImageGenerator.Core.EmailSettings;
using ImageGenerator.Core.Entities.Identity;
using ImageGenerator.Core.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;

namespace ImageGenerator.API.Controllers
{
    public class AccountController : ApiBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly DownloadImages _downloadImages;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 ITokenService tokenService,
                                 IEmailService emailService,
                                 DownloadImages downloadImages
                                 )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _downloadImages = downloadImages;
        }

        #region Register
        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register([FromForm] RegisterDTO model)
        {
            if (CheckEmailExisting(model.Email).Result.Value)
            {
                return BadRequest(new ApiResponse(400, "This Account Already Exists"));
            }

            DateTime parsedDate;
            if (!DateTime.TryParseExact(model.Birthday, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return BadRequest(new ApiResponse(400, "Invalid date format. Please use MM-dd-yyyy."));
            }

            var user = new AppUser()
            {
                Email = model.Email,
                Name = model.Name,
                UserName = model.Email.Split('@')[0],
                PhoneNumber = model.PhoneNumber,
                Birthday = parsedDate,
                Location = model.Location,
                ImageName = DocumentSettings.UploadFile(model.Image, "ProfilePictures"),
            };

            var Result = await _userManager.CreateAsync(user, model.Password);
            if (!Result.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "Registeration Failed."));
            }

            var ReturnedUser = new UserDTO()
            {
                Email = model.Email,
                Name = model.Name,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
                ImageName = user.ImageName,
                Birthday = parsedDate,
                Image = model.Image,
            };
            return Ok(ReturnedUser);
        }
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                var Flag = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (Flag.Succeeded)
                {
                    var returned = new UserDTO()
                    {
                        Email = user.Email,
                        Name = user.Name,
                        ImageName = user.ImageName,
                        Birthday = user.Birthday,
                        PhoneNumber = user.PhoneNumber,
                        Token = await _tokenService.CreateTokenAsync(user, _userManager),
                    };
                    return Ok(returned);
                }
                else
                {
                    return Unauthorized(new ApiResponse(400, "Enter a correct Password , please."));
                }
            }
            else
            {
                return Unauthorized(new ApiResponse(400, "This Email doesn't exist."));
            }
        }
        #endregion

        #region UpdateProfileImage
        [HttpPut("UpdateProfileImage")]
        public async Task<ActionResult> UpdateProfile([FromForm] UpdateProfileDTO image)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                DocumentSettings.DeleteFile(user.ImageName, "ProfilePictures");
                user.ImageName = DocumentSettings.UploadFile(image.Image, "ProfilePictures");
                var res = await _userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, "Updated Successfully!");
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "Error Updating data."));
                }
            }
            return BadRequest(new ApiResponse(400, "User not found"));

        }
        #endregion

        #region UpdateBirthday
        [HttpPut("UpdateBirthday")]
        public async Task<ActionResult> UpdateBirthday([FromBody] UpdateBirthdayDTO date)
        {
            DateTime parsedDate;
            if (!DateTime.TryParseExact(date.Date, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return BadRequest(new ApiResponse(400, "Invalid date format. Please use MM-dd-yyyy."));
            }
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                user.Birthday = parsedDate;
                var res = await _userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    return Ok(new { StatusCodes.Status200OK, user.Email, user.Birthday });
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "Error Updating Birthday."));
                }
            }
            return BadRequest(new ApiResponse(404, "User not found"));

        }
        #endregion

        #region Update PhoneNumber
        [HttpPut("UpdatePhoneNumber")]
        public async Task<ActionResult> UpdatePhoneNumber([FromBody] PhoneNumberDTO dto)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                user.PhoneNumber = dto.PhoneNumber;
                var res = await _userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    return Ok(new { StatusCodes.Status200OK, user.Email, user.PhoneNumber });
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "Error Updating PhoneNumber."));
                }
            }
            return BadRequest(new ApiResponse(404, "User not found"));

        }
        #endregion

        #region Update Name
        [HttpPut("UpdateName")]
        public async Task<ActionResult> UpdateName([FromBody] UpdateNameDTO dto)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                user.Name = dto.Name;
                var res = await _userManager.UpdateAsync(user);
                if (res.Succeeded)
                {
                    return Ok(new { StatusCodes.Status200OK, user.Email, user.Name });
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "Error Updating Name."));
                }
            }
            return BadRequest(new ApiResponse(404, "User not found"));

        }
        #endregion

        #region DeleteAccount
        [HttpDelete("Delete")]
        [Authorize]
        public async Task<ActionResult> DeleteAccount()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                DocumentSettings.DeleteFile(user.ImageName, "ProfilePictures");
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", user.UserName);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
                var res = await _userManager.DeleteAsync(user);
                if (res.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, "Deleted Successfully!");
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "Error deleting data."));
                }
            }
            else
            {
                return BadRequest(new ApiResponse(400, "User not found"));
            }
        }
        #endregion

        #region SignOut
        [HttpGet("SignOut")]
        [Authorize]
        public new async Task<ActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return StatusCode(StatusCodes.Status200OK, "Successfully Logged out.");
        }
        #endregion

        #region GetCurrentUser
        [HttpGet("GetCurrentUser")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            var returned = new UserDTO()
            {
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Birthday = user.Birthday,
                ImageName = user.ImageName,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
            };
            return Ok(returned);
        }
        #endregion

        #region emailExists
        [HttpGet("emailExists")]
        public async Task<ActionResult<bool>> CheckEmailExisting([Required] string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        #endregion

        #region Reset Password
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult> ForgorPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                var Token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordLink = Url.Action(nameof(ResetPassword), "Account", new { Token, email = user.Email }, Request.Scheme);
                var emailToSend = new Email()
                {
                    To = user.Email,
                    Subject = "Reset Password",
                    Body = $"Please reset your password by clicking here: <a href=\"{resetPasswordLink}\">link</a>",
                };
                try
                {
                    _emailService.SendEmail(emailToSend);
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse(200, "Email has been Sent"));
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Failed to send email: {ex.Message}");
                }
            }
            else
            {
                return BadRequest(new ApiResponse(404, "User not found"));
            }
        }

        [HttpGet("ResetPassword")]
        public async Task<ActionResult> ResetPassword(string email, string token)
        {
            var model = new ResetPasswordDTO()
            {
                Email = email,
                Token = token,
            };
            return Ok(model);
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is not null)
            {
                var resetPassword = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
                if (!resetPassword.Succeeded)
                {
                    foreach (var error in resetPassword.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }
                else
                {
                    return StatusCode(StatusCodes.Status200OK, new ApiResponse(200, "Password has been changed"));
                }
            }
            return BadRequest(new ApiResponse(400, "couldn't send email"));
        }

        #endregion

        #region ChangePassword
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
            {
                return BadRequest(new ApiResponse(404, "User Is not Existing"));

            }
            if (string.Compare(dto.NewPassword, dto.ConfirmNewPassword) != 0)
            {
                return BadRequest(new ApiResponse(400, "NewPassword and ConfirmNewPassword doesn't match"));
            }
            var Reuslt = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!Reuslt.Succeeded)
            {
                foreach (var error in Reuslt.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Ok(ModelState);
            }

            return Ok(new ApiResponse(500, "Password has been changed"));
        }
        #endregion



    }
}

#region Temp

/*
 //Return 1
[HttpGet("Now")]
public async Task<IActionResult> GetAllImages()
{
    var email = User.FindFirstValue(ClaimTypes.Email);
    var user = await _userManager.FindByEmailAsync(email);

    // Combine the directory path with the wwwroot folder
    string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.UserName);

    // Check if the directory exists
    if (!Directory.Exists(imagesDirectory))
    {
        // Return 404 Not Found if directory doesn't exist
        return NotFound();
    }

    // Get all image file paths in the directory
    string[] imageFiles = Directory.GetFiles(imagesDirectory)
                                   .Select(Path.GetFileName)
                                   .ToArray();

    // Create a list to store concatenated image data
    List<byte> imageData = new List<byte>();

    // Read each image file and append its data to the list
    foreach (var fileName in imageFiles)
    {
        var filePath = Path.Combine(imagesDirectory, fileName);
        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        imageData.AddRange(fileBytes);
    }

    // Convert the list to a byte array
    byte[] combinedImageData = imageData.ToArray();

    // Return the combined image data as a file with the correct MIME type
    return File(combinedImageData, "image/png");
}


 */
#endregion