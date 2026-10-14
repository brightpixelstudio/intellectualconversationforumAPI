using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class FormUpdatePost
    {
        [Key]
        public string? postguid { get; set; }

        [Required(ErrorMessage = "Post Catagory Id is required.")]
        public int postcatagoryid { get; set; }

        [Required(ErrorMessage = "Post is required.")]
        [MaxLength(5000, ErrorMessage = "The post must be less than 5000 characters long.")]
        public string? post { get; set; }
    }
}
