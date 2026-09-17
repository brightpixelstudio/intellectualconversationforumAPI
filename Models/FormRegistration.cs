using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class FormRegistration
    {
        [Key]
        public int userid { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MinLength(8, ErrorMessage = "The name must be at least 8 characters long.")]
        public string? name { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(7, ErrorMessage = "The username must be at least 7 characters long.")]
        public string? username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? email { get; set; }

        [Required(ErrorMessage = "Zipcode is required.")]
        public int zip { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string? password { get; set; }

        [Required(ErrorMessage = "Profile is required.")]
        [MinLength(20, ErrorMessage = "The profile must be at least 30 characters long.")]
        [MaxLength(1000, ErrorMessage = "The profile must be less than 1000 characters long.")]
        public string? profile { get; set; }
    }
}
