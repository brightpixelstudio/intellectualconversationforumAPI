namespace intellectualconversationAPI.Models.Announcements
{
    public class Announcement
    {
        public int announcementid { get; set; }
        public string title { get; set; } = string.Empty;
        public string announcement { get; set; } = string.Empty;
        public DateTime dateadded { get; set; }
        public DateTime? dateexpire { get; set; }
    }
}
