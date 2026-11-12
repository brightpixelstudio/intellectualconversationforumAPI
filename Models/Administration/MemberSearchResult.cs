using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models.Administration
{
    public class MemberSearchResult
    {
        [Key]
        public string userguid { get; set; } = "";
        public string name { get; set; } = "";
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public int usertypeid { get; set; }
        public string type { get; set; } = "";
    }
}
