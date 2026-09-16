using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetPostsMember
    {         
        [Key]
        public int postid { get; set; }
        public int userid { get; set; }
        public int postcatagoryid { get; set; }
        public string catagory { get; set; }
        public DateTime dateadded { get; set; }
        public int count { get; set; }
        public string shorttext { get; set; }
    }
}
