using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetPostRatingsMember
    {
        [Key]
        public int postid { get; set; }
        public int rating { get; set; }
        public DateTime ratingdateadded { get; set; }
        public DateTime ratingdateupdated { get; set; }
        public string? postguid { get; set; }
        public string? postcontent { get; set; }
        public int postauthorid { get; set; }
        public string? postauthorname { get; set; }
        public string? postauthorguid { get; set; }
        public int postcatagoryid { get; set; }
        public string? postcatagory { get; set; }
        public float postaveragerating { get; set; }
        public int commentcount { get; set; }
        public DateTime? dateclosed { get; set; }
        public DateTime postdateadded { get; set; }
    }
}
