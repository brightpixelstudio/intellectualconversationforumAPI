using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class PostRatingResponse
    {         
        [Key]
        public int Postid { get; set; }
        public decimal AverageRating { get; set; }
    }
}
