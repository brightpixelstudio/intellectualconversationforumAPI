using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetUserList
    {
        [Key]
        public int userid { get; set; }
        public string? name { get; set; }
    }
}
