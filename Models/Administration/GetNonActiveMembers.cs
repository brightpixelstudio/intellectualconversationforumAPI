using System.ComponentModel.DataAnnotations;

namespace intellectualconversationforumAPI.Models.Administration
{
    public class GetNonActiveMembers
    {
        [Key]
        public int userid { get; set; }
        public string userguid { get; set; } = string.Empty;
        public int accountstatusid { get; set; }
        public string username { get; set; } = "";
        public string accountstatus { get; set; } = "";
        public DateTime datechanged { get; set; }
        public int changedbyuserid { get; set; }
        public string reason { get; set; } = "";
        public string changedbyname { get; set; } = "";
    }
}
