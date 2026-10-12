using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Statsitics
{   
    // Updates
    public class GetHighestRatingPosts
    {
        [Key]
        public int postid { get; set; }
        public int userid { get; set; }
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }
        public string? shorttextpost { get; set; }
        public int count { get; set; }
        public float rating { get; set; }
    }
}
