namespace intellectualconversationforumAPI.Models.Engagement
{
    public class MemberParticipationPattern
    {
        public int UserId { get; set; }

        public string Guid { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? ProfileImageKey { get; set; }

        public int Zipcode { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string ParticipationType { get; set; } = string.Empty;

        public int PostsInRange { get; set; }

        public int CommentsInRange { get; set; }

        public int LifetimePosts { get; set; }

        public int LifetimeComments { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public string? ProfileImageUrl { get; set; }
    }
}
