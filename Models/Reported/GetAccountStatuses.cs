using System.ComponentModel.DataAnnotations;

namespace intellectualconversationforumAPI.Models.Reported
{
    public class GetReportedStatuses
    {
        [Key]
        public int reportedstatusid { get; set; }
        public string? statusname { get; set; }
    }
}
