using Microsoft.EntityFrameworkCore;

namespace intellectualconversationAPI.Models.Engagement
{
    public class CommunityOverviewRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    [Keyless]
    public class CommunityOverview
    {
        public int TotalMembers { get; set; }
        public int NewMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int MembersCommenting { get; set; }
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
    }
}
