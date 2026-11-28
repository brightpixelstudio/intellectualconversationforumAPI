namespace intellectualconversationforumAPI.Models.Engagement
{
    public class EngagementMemberParticipationResponse
    {
        public MemberParticipationSummary Summary { get; set; } = new();

        public List<MemberParticipationActivity> MostActiveMembers { get; set; } = new();

        public List<MemberParticipationPattern> ParticipationPatterns { get; set; } = new();

        public List<MemberParticipationRetention> MemberRetention { get; set; } = new();
    }
}
