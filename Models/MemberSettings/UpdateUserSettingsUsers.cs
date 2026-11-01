
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class UpdateUserSettingsUsersRequest
    {
        public string guid { get; set; } = string.Empty;

        public List<UserSettingsUserItem> users { get; set; } = new();
    }

    public class UserSettingsUserItem
    {
        public int userid { get; set; }

        public int selected { get; set; }
    }
}
