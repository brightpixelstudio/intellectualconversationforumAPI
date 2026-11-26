using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class FormIdeaUpdate
    {
        public string ResponseUserGuid { get; set; } = string.Empty; 
        public string IdeaGuid { get; set; } = string.Empty;
        public int IdeaStatusId { get; set; }
        public string? Response { get; set; }
    }
}
