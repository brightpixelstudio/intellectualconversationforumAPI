
using Microsoft.EntityFrameworkCore;

namespace intellectualconversationAPI.Models
{
    [Keyless]
    public class FormUpdateUserSettings
    {
        public int minpostrating { get; set; }
        public sbyte showunrated { get; set; }
        public sbyte addnewusers { get; set; }
        public sbyte addnewcatagories { get; set; }
    }
}
