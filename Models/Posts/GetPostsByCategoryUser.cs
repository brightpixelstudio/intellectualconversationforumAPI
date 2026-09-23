using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class GetPostsByCategoryUser
    {
        [Key]
        public int postid { get; set; }        
        public int userid { get; set; }
        public string? username { get; set; }
        public string? name { get; set; }
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }        
        public DateTime dateadded { get; set; }
        public int count { get; set; }
        public string? post { get; set; }
    }
}
