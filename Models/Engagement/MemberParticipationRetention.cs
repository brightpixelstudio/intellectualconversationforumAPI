namespace intellectualconversationforumAPI.Models.Engagement
{
    public class MemberParticipationRetention
    {
        public int UserId { get; set; }

        public string Guid { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? ProfileImageKey { get; set; }

        public int Zipcode { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public DateTime DateAdded { get; set; }

        public string RetentionType { get; set; } = string.Empty;

        public int LifetimePosts { get; set; }

        public int LifetimeComments { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public string? ProfileImageUrl { get; set; }
    }
}
