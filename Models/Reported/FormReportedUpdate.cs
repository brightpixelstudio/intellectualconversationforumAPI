namespace intellectualconversationforumAPI.Models.Reports
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class FormReportedUpdate
    {
        public int reportedid { get; set; }
        public string? response { get; set; }
        public int reportedstatusid { get; set; }
        public string? userguid { get; set; }
    }
}

