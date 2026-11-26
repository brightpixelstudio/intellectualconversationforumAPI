namespace intellectualconversationforumAPI.Models.Idea
{
    public class FormGetIdeas
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int IdeaTypeId { get; set; }
        public int IdeaStatusId { get; set; }
    }
}
