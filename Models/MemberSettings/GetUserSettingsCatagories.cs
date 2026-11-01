using Microsoft.EntityFrameworkCore;

namespace intellectualconversationAPI.Models
{
    [Keyless]
    public class GetUserSettingsCatagories
    {
        public int? usersettingscatagoryid { get; set; }
        public int userid { get; set; }
        public int postcatagoryid { get; set; }
        public string? catagory { get; set; }
        public int selected { get; set; }        
    }
}
