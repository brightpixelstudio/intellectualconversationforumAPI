using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class IsEmailAndUsernameUsed
    {
        [Key]
        public int id { get; set; }
        public int result { get; set; }
    }
}
