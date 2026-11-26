using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class FormIdeaNew
    {
        public string UserGuid { get; set; } = string.Empty;
        public int IdeaTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
