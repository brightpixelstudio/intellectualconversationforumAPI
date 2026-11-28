using Microsoft.EntityFrameworkCore;

namespace intellectualconversationforumAPI.Models.Notification
{
    [Keyless]
    public class NotificationUnreadCount
    {
        public int UnreadCount { get; set; }
    }
}
