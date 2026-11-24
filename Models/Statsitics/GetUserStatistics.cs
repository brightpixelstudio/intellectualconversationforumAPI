using Microsoft.EntityFrameworkCore;

namespace intellectualconversationAPI.Models.Statsitics
{   
    [Keyless]
    public class GetStatisticsMember
    {
        public int UserId { get; set; }
        public string UserGuid { get; set; } = string.Empty;

        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }

        public decimal AveragePostRating { get; set; }
        public decimal AverageCommentRating { get; set; }

        public int TotalPostsRated { get; set; }
        public int TotalCommentsRated { get; set; }
    }
}
