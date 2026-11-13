using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    // GetCommentsMember
    public class GetCommentsMember
    {
    
        [Key]
        public int postcommentid { get; set; }
        public int postid { get; set; }
        public string? postguid { get; set; }
        public string? userguid { get; set; }
        public string? commentauthor { get; set; }
        public string? postuserguid { get; set; }
        public string? postauthor { get; set; }
        public string? catagory { get; set; }
        public int postcatagoryid { get; set; }        
        public string? commentshorttext { get; set; }
        public float rating { get; set; }
        public DateTime dateadded { get; set; }
        public sbyte commentsclosed { get; set; }
    }
}
