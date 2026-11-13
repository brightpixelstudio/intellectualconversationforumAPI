namespace intellectualconversationAPI.Data
{
    using intellectualconversationAPI.Models;
    using intellectualconversationAPI.Models.Posts;
    using intellectualconversationAPI.Models.Member;
    using intellectualconversationAPI.Models.Statsitics;
    using intellectualconversationAPI.Models.Administration;
    using Microsoft.EntityFrameworkCore;
    

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // users
        public DbSet<GetPostsMember> GetPostsMember { get; set; }
        public DbSet<GetCommentsMember> GetCommentsMember { get; set; }
        public DbSet<GetPostRatingsMember> GetPostRatingsMember { get; set; }
        public DbSet<GetCommentRatingsMember> GetCommentRatingsMember { get; set; }
        public DbSet<GetAccountStatusHistoryMember> GetAccountStatusHistoryMember { get; set; }        
        public DbSet<GetPostsWithMostCommentsMember> GetPostsWithMostCommentsMember { get; set; }
        public DbSet<GetProfileMember> GetProfileMember { get; set; }
        public DbSet<GetProfileMemberImage> GetProfileMemberImage { get; set; }        
        public DbSet<GetAllProfileMembers> GetAllProfileMembers { get; set; }
        public DbSet<FormRegistration> FormRegistration { get; set; }
        public DbSet<GetUserList> GetUserList { get; set; }
        public DbSet<GetAccountStatuses> GetAccountStatuses { get; set; }        
        public DbSet<UserForLogin> UserForLogin { get; set; }

        // users settings
        public DbSet<GetUserSettings> GetUserSettings { get; set; }
        public DbSet<GetUserSettingsCatagories> GetUserSettingsCatagories { get; set; }
        public DbSet<GetUserSettingsUser> GetUserSettingsUser { get; set; }


        // statistics
        public DbSet<GetMostPopularCatagories> GetMostPopularCatagories { get; set; }
        public DbSet<GetMostPostsMembers> GetMostPostsMembers { get; set; }
        public DbSet<GetMostPopularPostWithComments> GetMostPopularPostWithComments { get; set; }
        public DbSet<GetHighestRatingPosts> GetHighestRatingPosts { get; set; }        
        public DbSet<GetLatestLoginsMembers> GetLatestLoginsMembers { get; set; }
        public DbSet<GetNewestMembers> GetNewestMembers { get; set; }

        // posts
        public DbSet<GetPostsByCategoryUserRating> GetPostsByCategoryUserRating { get; set; }
        public DbSet<GetPostComments> GetPostComments { get; set; }
        public DbSet<GetRatings> GetRatings { get; set; }
        public DbSet<FormPostNew> FormPostNew { get; set; }
        public DbSet<GetPost> GetPost { get; set; }
        public DbSet<PostImage> PostImage { get; set; }
        public DbSet<GetComment> GetComment { get; set; }

        // utilities
        public DbSet<IsEmailAndUsernameUsed> IsEmailAndUsernameUsed { get; set; }
        public DbSet<GetCatagoryList> GetCatagoryList { get; set; }
        public DbSet<GetVerifyZipcode> GetVerifyZipcode { get; set; }

        // admin
    }
}
