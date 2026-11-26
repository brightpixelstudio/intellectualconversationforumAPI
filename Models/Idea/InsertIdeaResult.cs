using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class InsertIdeaResult
    {
        public int ideaid { get; set; }
        public string guid { get; set; } = string.Empty;
    }
}
