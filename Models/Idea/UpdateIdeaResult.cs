using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class UpdateIdeaResult
    {
        public int ideaid { get; set; }
        public string guid { get; set; } = string.Empty;
        public int ideastatusid { get; set; }
        public string? response { get; set; }
        public DateTime? dateupdated { get; set; }
    }
}
