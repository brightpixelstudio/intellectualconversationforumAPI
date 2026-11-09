using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class GetPostsByCategoryUserRating
    {
        [Key]
        public int postid { get; set; }        
        public int userid { get; set; }
        public string? postguid { get; set; }
        public string? userguid { get; set; }
        public string? username { get; set; }
        public string? name { get; set; }
        public string? profileimagekey { get; set; }
        public string? profileimageurl { get; set; }
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }        
        public DateTime dateadded { get; set; }
        public int count { get; set; }
        public string? post { get; set; }
        public float rating { get; set; }
        public int ratingcount { get; set; }
        public int? userrating { get; set; }
        public sbyte commentsclosed { get; set; }
        public int usertypeid { get; set; }
        public string? type { get; set; }
        public bool ispinned { get; set; }
        public DateTime? datepinned { get; set; }
        public int? pinnedbyuserid { get; set; }

        public List<PostImage> postimages { get; set; } = new();
    }
}
