using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Statsitics
{   
    // Updates
    public class GetMostPopularPostWithComments
    {
        [Key]
        public int postid { get; set; }
        public string? postguid { get; set; }
        public string? userguid { get; set; }
        public int userid { get; set; }
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }
        public string? shorttextpost { get; set; }
        public int count { get; set; }
        public float rating { get; set; }
        public sbyte commentsclosed { get; set; }
        public string? name { get; set; }
        public string? profileimagekey { get; set; }
        public string? profileimageurl { get; set; }
    }
}
