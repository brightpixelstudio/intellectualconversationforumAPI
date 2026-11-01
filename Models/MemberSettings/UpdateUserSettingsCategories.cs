
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationAPI.Models
{
    public class UpdateUserSettingsCategoriesRequest
    {
        public string guid { get; set; } = string.Empty;

        public List<UserSettingsCategoryItem> categories { get; set; } = new();
    }

    public class UserSettingsCategoryItem
    {
        public int postcatagoryid { get; set; }

        public int selected { get; set; }
    }
}
