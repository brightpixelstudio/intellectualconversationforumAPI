using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetVerifyZipcode
    {
        [Key]
        public sbyte isValid { get; set; }
    }
}
