using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class PostImage
    {
        [Key]
        public int postimageid { get; set; }
        public int postid { get; set; }
        public string postimagekey { get; set; } = string.Empty;
        public string? postimageurl { get; set; }
    }
}
