namespace intellectualconversationAPI.Data
{
    using intellectualconversationAPI.Models;
    using Microsoft.EntityFrameworkCore;
    
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // users
        public DbSet<GetLatestLoginsMembers> GetLatestLoginsMembers { get; set; }
        public DbSet<GetMostPostsMembers> GetMostPostsMembers { get; set; }
        public DbSet<GetNewestMembers> GetNewestMembers { get; set; }
        public DbSet<GetPostsMember> GetPostsMember { get; set; }
        public DbSet<GetPostsWithMostCommentsMember> GetPostsWithMostCommentsMember { get; set; }
        public DbSet<GetProfileMember> GetProfileMember { get; set; }
        public DbSet<GetAllProfileMembers> GetAllProfileMembers { get; set; }
        public DbSet<FormRegistration> FormRegistration { get; set; }
        

        // statistics
        public DbSet<GetMostPopularCatagories> GetMostPopularCatagories { get; set; }
        public DbSet<GetMostPopularPostWithComments> GetMostPopularPostWithComments { get; set; }

        // posts
        public DbSet<GetPostsByCategory> GetPostsByCategory { get; set; }
        public DbSet<GetPostComments> GetPostComments { get; set; }

        // utilities
        public DbSet<IsEmailAndUsernameUsed> IsEmailAndUsernameUsed { get; set; }        
    }
}
