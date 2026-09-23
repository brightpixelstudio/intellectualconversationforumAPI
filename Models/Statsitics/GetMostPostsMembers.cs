using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Statsitics
{
    public class GetMostPostsMembers
    {         
        [Key]
        public int userid { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public int zipcode { get; set; }
        public string? profile { get; set; }
        public DateTime dateadded { get; set; }
        public DateTime datelastlogin { get; set; }
        public int count { get; set; }
    }
}
