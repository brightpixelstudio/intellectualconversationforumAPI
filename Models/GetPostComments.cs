using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetPostComments
    {
        [Key]
        public int postid { get; set; }
        public int userid { get; set; }
        public string username { get; set; }
        public string? name { get; set; }
        public DateTime dateadded { get; set; }
        public string comment { get; set; }
    }
}
