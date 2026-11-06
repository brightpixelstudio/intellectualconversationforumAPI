using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class PostRatingResponse
    {         
        [Key]
        public int Postid { get; set; }
        public decimal RatingAverage { get; set; }
        public int? UserRating { get; set; }        
        public int RatingCount{ get; set; }
    }
}
