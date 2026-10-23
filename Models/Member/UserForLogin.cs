
using Microsoft.EntityFrameworkCore;

namespace intellectualconversationAPI.Models
{
    [Keyless]
    public class UserForLogin
    {
        public int userid { get; set; }
        public string guid { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public bool blocked { get; set; }
        public DateTime? suspended { get; set; }
    }
}
