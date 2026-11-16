using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Administration
{
    public class MemberSearchResult
    {
        [Key]
        public string userguid { get; set; } = "";
        public int userid { get; set; }
        public int usertypeid { get; set; }
        public string type { get; set; } = "";
        public int accountstatusid { get; set; }
        public string accountstatus { get; set; } = "";
        public string name { get; set; } = "";
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public int zipcode { get; set; }
        public int postcount { get; set; }
        public string profile { get; set; } = "";
        public DateTime? datelastpost { get; set; }
        public DateTime? datelastlogin { get; set; }
        public DateTime? dateadded { get; set; }
        public string? profileimagekey { get; set; }
        public string? profileimageurl { get; set; } = "";

    }
}
