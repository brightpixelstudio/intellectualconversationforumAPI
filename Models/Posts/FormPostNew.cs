using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class FormPostNew
    {
        [Key]
        [Required(ErrorMessage = "Catagory ID is required.")]
        public int catagoryid { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int userid { get; set; }

        [Required(ErrorMessage = "Post is required.")]
        [MaxLength(5000, ErrorMessage = "The post must be less than 5000 characters long.")]
        public string? post { get; set; }
    }
}
