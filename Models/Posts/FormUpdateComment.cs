using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class FormUpdateComment
    {
        [Key]
        public int commentid { get; set; }

        [Required(ErrorMessage = "User Id is required.")]
        public int userid { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [MaxLength(5000, ErrorMessage = "The comment must be less than 5000 characters long.")]
        public string? comment { get; set; }
    }
}
