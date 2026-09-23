namespace intellectualconversationAPI.Data
{
    using intellectualconversationAPI.Models;
    using intellectualconversationAPI.Models.Posts;
    using intellectualconversationAPI.Models.Member;
    using intellectualconversationAPI.Models.Statsitics;
    using Microsoft.EntityFrameworkCore;
    
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // users
        public DbSet<GetPostsMember> GetPostsMember { get; set; }
        public DbSet<GetCommentsMember> GetCommentsMember { get; set; }
        public DbSet<GetPostsWithMostCommentsMember> GetPostsWithMostCommentsMember { get; set; }
        public DbSet<GetProfileMember> GetProfileMember { get; set; }
        public DbSet<GetAllProfileMembers> GetAllProfileMembers { get; set; }
        public DbSet<FormRegistration> FormRegistration { get; set; }
        public DbSet<GetUserList> GetUserList { get; set; }        

        // statistics
        public DbSet<GetMostPopularCatagories> GetMostPopularCatagories { get; set; }
        public DbSet<GetMostPostsMembers> GetMostPostsMembers { get; set; }
        public DbSet<GetMostPopularPostWithComments> GetMostPopularPostWithComments { get; set; }
        public DbSet<GetLatestLoginsMembers> GetLatestLoginsMembers { get; set; }
        public DbSet<GetNewestMembers> GetNewestMembers { get; set; }

        // posts
        public DbSet<GetPostsByCategoryUser> GetPostsByCategoryUser { get; set; }
        public DbSet<GetPostComments> GetPostComments { get; set; }
        public DbSet<FormPostNew> FormPostNew { get; set; }

        // utilities
        public DbSet<IsEmailAndUsernameUsed> IsEmailAndUsernameUsed { get; set; }
        public DbSet<GetCatagoryList> GetCatagoryList { get; set; }
    }
}
