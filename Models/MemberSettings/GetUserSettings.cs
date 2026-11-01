
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class GetUserSettings
    {
        [Key]
        public int usersettingsid { get; set; }
        public int userid { get; set; }
        public float minpostrating { get; set; }
        public sbyte showunrated { get; set; }
        public sbyte addnewusers { get; set; }
        public sbyte addnewcatagories { get; set; }
        public DateTime? dateupdated { get; set; }
    }
}
