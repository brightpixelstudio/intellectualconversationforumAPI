using System.ComponentModel.DataAnnotations;

namespace intellectualconversationforumAPI.Models.Notification
{
    public class Notification
    {
        [Key]
        public long NotificationId { get; set; }
        public int RecipientUserId { get; set; }
        public int ActorUserId { get; set; }
        public string? ActorUsername { get; set; }
        public string? ActorName { get; set; }
        public string? ActorGuid { get; set; }
        public string? PostShortText { get; set; }
        public int PostId { get; set; }
        public uint PostCommentId { get; set; }
        public bool IsRead => DateRead != null;
        public DateTime DateCreated { get; set; }
        public DateTime? DateRead { get; set; }
        public string PostGuid { get; set; } = string.Empty;
        public string? ProfileImageKey { get; set; }
        public string? ProfileImageUrl { get; set; }

    }
}
