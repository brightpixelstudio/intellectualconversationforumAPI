using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace intellectualconversationforumAPI.Models.Notification
{
    [Keyless]
    public class NotificationUnreadCount
    {
        public int UnreadCount { get; set; }
    }
}
