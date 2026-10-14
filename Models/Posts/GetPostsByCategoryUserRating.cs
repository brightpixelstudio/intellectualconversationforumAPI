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
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }        
        public DateTime dateadded { get; set; }
        public int count { get; set; }
        public string? post { get; set; }
        public float rating { get; set; }
        public sbyte commentsclosed { get; set; }
    }
}
