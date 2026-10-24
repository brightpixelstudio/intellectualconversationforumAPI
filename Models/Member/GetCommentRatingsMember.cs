using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetCommentRatingsMember
    {
        [Key]
        public int commentrating { get; set; }
        public DateTime ratingdateadded { get; set; }
        public DateTime ratingdateupdated { get; set; }
        public int postcommentid { get; set; }
        public string? commentguid { get; set; }
        public string? commentcontent { get; set; }
        public int commentauthorid { get; set; }
        public string? commentauthorname { get; set; }
    }
}
