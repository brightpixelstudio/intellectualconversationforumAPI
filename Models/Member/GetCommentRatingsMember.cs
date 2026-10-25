using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetCommentRatingsMember
    {
        [Key]
        public int postcommentid { get; set; }
        public int rating { get; set; }
        public DateTime ratingdateadded { get; set; }
        public DateTime? ratingdateupdated { get; set; }
        public string? commentguid { get; set; }
        public string? commentcontent { get; set; }
        public int commentauthorid { get; set; }
        public string? commentauthorname { get; set; }
        public string? commentauthorguid { get; set; }
        public int postid { get; set; }
        public string? postguid { get; set; }
        public int postcatagoryid { get; set; }
        public string? postcatagory { get; set; }
        public float commentaveragerating { get; set; }
        
    }
}
