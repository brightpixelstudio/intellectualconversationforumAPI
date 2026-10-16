using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Posts
{
    public class GetPostRatings
    {
        [Key]
        public int userid { get; set; }
        public int rating { get; set; }
        public DateTime dateupdated { get; set; }
        public DateTime dateadded { get; set; }
        public string? name { get; set; }
    }
}
