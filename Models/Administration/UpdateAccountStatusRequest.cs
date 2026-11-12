using System.ComponentModel.DataAnnotations;

namespace intellectualconversationforumAPI.Models.Administration
{

    public class UpdateAccountStatusRequest
    {
        public int userid { get; set; }
        public int accountstatusid { get; set; }
        public string reason { get; set; } = string.Empty;
    }
}
