using System.ComponentModel.DataAnnotations;

namespace BlogLab.Models.Account
{
    public class ApplicationUserCreate : ApplicationUserLogin
    {
        [MinLength(10, ErrorMessage = "Must be at least 10 characters")]
        [MaxLength(30, ErrorMessage = "Must be at most 30 characters")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(20, ErrorMessage = "Must be at most 30 characters")]
        [EmailAddress(ErrorMessage = "Not a valid email format")]
        public string Email { get; set; }
    }
}