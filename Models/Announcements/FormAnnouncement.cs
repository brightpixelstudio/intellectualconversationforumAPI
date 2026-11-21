namespace intellectualconversationAPI.Models.Announcements
{
    using System.ComponentModel.DataAnnotations;
    
    public class FormAnnouncement
    {
        [Key]
        public int announcementid { get; set; }

        [Required]
        [StringLength(200)]
        public string title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string announcement { get; set; } = string.Empty;

        public DateTime? dateexpire { get; set; }
    }
}
