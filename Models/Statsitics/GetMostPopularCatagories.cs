using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Statsitics
{
    public class GetMostPopularCatagories
    {            
        [Key]
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }
        public int count { get; set; }
    }
}
