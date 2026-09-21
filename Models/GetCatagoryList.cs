using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetCatagoryList
    {
        [Key]
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }
    }
}
