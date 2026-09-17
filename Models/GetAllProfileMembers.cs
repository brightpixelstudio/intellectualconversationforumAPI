using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetAllProfileMembers
    {
        [Key]
        public int userid { get; set; }
        public int usertypeid { get; set; }
        public string? type { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public int zipcode { get; set; }
        public int count { get; set; }
        public string? profile { get; set; }
        public DateTime? datelastpost { get; set; }
        public DateTime datelastlogin { get; set; }
        public DateTime dateadded { get; set; }
        public bool blocked { get; set; }
    }
}
