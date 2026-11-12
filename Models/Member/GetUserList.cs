
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Member
{
    public class GetAccountStatuses
    {
        [Key]
        public int accountstatusid { get; set; }
        public string? accountstatus { get; set; }
    }
}
