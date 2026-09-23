using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class GetPostComments
    {
        [Key]
        public int postcommentid { get; set; }
        public int postid { get; set; }
        public int userid { get; set; }
        public string? name { get; set; }
        public string username { get; set; }
        public DateTime dateadded { get; set; }
        public string catagory { get; set; }
        public string comment { get; set; }
    }
}
