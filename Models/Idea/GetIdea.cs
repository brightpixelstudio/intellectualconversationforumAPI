using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Idea
{
    [Keyless]
    public class GetIdea
    {
        public string IdeaGuid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int IdeaTypeId { get; set; }
        public string IdeaType { get; set; } = string.Empty;
        public int IdeaStatusId { get; set; }
        public string IdeaStatus { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserGuid { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? ProfileImageKey { get; set; }
        public string? profileimageurl { get; set; }
        public string? Response { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
