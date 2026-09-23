using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetCommentsMember
    {
    
        [Key]
        public int postid { get; set; }
        public string? catagory { get; set; }
        public int postcommentid { get; set; }
        public string? postshorttext { get; set; }
        public string? commentshorttext { get; set; }
        public DateTime dateadded { get; set; }
    }
}
