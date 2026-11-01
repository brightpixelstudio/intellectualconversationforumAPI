
using Microsoft.EntityFrameworkCore;

namespace intellectualconversationAPI.Models
{
    [Keyless]
    public class GetUserSettingsUser
    {
        public int? usersettingsuserid { get; set; }
        public int userid { get; set; }
        public int selecteduserid { get; set; }
        public string? guid { get; set; }
        public string? name { get; set; }
        public string? username { get; set; }
        public int selected { get; set; }
    }
}
