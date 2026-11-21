namespace intellectualconversationforumAPI.Models.Reports
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class FormReported
    {
        [Key]
        public int reportedid { get; set; }

        public int reportedstatusid { get; set; } = 1;

        [Required]
        public string reporteduserguid { get; set; }

        public int? postid { get; set; }

        public int? postcommentid { get; set; }

        public string reportedcontent { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? response { get; set; }

        public DateTime? dateadded { get; set; }

        public DateTime? dateresponded { get; set; }

        public int? respondeduserid { get; set; }
    }
}

