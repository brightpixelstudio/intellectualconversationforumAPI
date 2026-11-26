using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class GetIdeaStatus
    {
        public int IdeaStatusId { get; set; }
        public string IdeaStatus { get; set; } = string.Empty;
    }
}
