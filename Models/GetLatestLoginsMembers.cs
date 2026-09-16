using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetLatestLoginsMembers
    {
        [Key]
        public int userid { get; set; }
        public string? username { get; set; }
        public string? name { get; set; }
        public int usertypeid { get; set; }
        public DateTime dateadded { get; set; }
        public DateTime datelastlogin { get; set; }
        public int postcount { get; set; }
        public string? profile { get; set; }
    }
}
