using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    // GetCommentsMember
    public class GetCommentsMember
    {
    
        [Key]
        public int postid { get; set; }
        public string? postguid { get; set; }
        public int userid { get; set; }
        public string? userguid { get; set; }
        public string? name { get; set; }
        public string? catagory { get; set; }
        public int postcatagoryid { get; set; }        
        public int postcommentid { get; set; }
        public string? postshorttext { get; set; }
        public string? commentshorttext { get; set; }
        public DateTime dateadded { get; set; }
        public sbyte commentsclosed{ get; set; }
    }
}
