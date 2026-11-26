using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class GetIdeaType
    {
        public int IdeaTypeId { get; set; }
        public string IdeaType { get; set; } = string.Empty;
    }
}
