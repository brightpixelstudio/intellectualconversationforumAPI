using System;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetAccountStatusHistoryMember
    {
        [Key]
        public int accountstatushistoryid { get; set; }
        public int userid { get; set; }
        public string? userguid { get; set; }
        public int accountstatusid { get; set; }
        public string? accountstatus { get; set; }
        public DateTime datechanged { get; set; }
        public int changedbyuserid { get; set; }
        public string? changedbyname { get; set; }
        public string? reason { get; set; }
    }
}
