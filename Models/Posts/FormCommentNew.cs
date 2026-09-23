using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class FormCommentNew
    {
        [Key]
        [Required(ErrorMessage = "Post ID is required.")]
        public int postid { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int userid { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [MaxLength(5000, ErrorMessage = "The comment must be less than 5000 characters long.")]
        public string? comment { get; set; }
    }
}
