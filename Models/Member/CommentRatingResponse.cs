using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class CommentRatingResponse
    {         
        [Key]
        public int Commentid { get; set; }
        public decimal AverageRating { get; set; }
        public int? UserRating { get; set; }
        public int RatingCount { get; set; }

    }
}
