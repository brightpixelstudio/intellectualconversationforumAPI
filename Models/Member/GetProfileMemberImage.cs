using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetProfileMemberImage
    {
        [Key]
        public string? profileimagekey { get; set; }        
    }
}
