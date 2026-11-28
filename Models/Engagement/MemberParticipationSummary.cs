
namespace intellectualconversationforumAPI.Models.Engagement
{
    public class MemberParticipationSummary
    {
        public int TotalMembers { get; set; }

        public int ActiveMembers { get; set; }

        public int MembersWhoPosted { get; set; }

        public int MembersWhoCommented { get; set; }

        public int PostOnlyMembers { get; set; }

        public int CommentOnlyMembers { get; set; }

        public int NeverParticipatedMembers { get; set; }

        public int InactiveMembers { get; set; }

        public int TotalPosts { get; set; }

        public int TotalComments { get; set; }
    }
}
