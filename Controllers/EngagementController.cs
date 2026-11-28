using Azure.Storage.Blobs;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Data.Helpers;
using intellectualconversationAPI.Models.Engagement;
using intellectualconversationAPI.Services;
using intellectualconversationforumAPI.Models.Engagement;
using IntellectualConversationForumAPI.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;
using System.Security.Claims;

namespace intellectualconversationforumAPI.Controllers
{
    // Controllers
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class EngagementController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PostController> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _postImagesContainer;
        private readonly BlobImageService _blobImageService;
        private readonly IValidateImageService _validateImageService;
        private readonly IConfiguration _configuration;

        public EngagementController(ILogger<PostController> logger, AppDbContext context, IConfiguration configuration, BlobImageService blobImageService, IValidateImageService validateImageService)
        {
            _logger = logger;
            _context = context;
            _blobImageService = blobImageService;
            _validateImageService = validateImageService;
            _configuration = configuration;

            _postImagesContainer =
                configuration["AzureStorage:PostImagesContainerName"]
                ?? throw new InvalidOperationException(
                    "Post images container is missing.");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetEngagementCommunityOverview(
            [FromBody] CommunityOverviewRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.StartDate == default || model.EndDate == default ||  model.StartDate.Date > model.EndDate.Date)
            {
                return BadRequest("Invalid date range.");
            }

            var connection = (MySqlConnection)_context.Database.GetDbConnection();

            bool closeConnection = connection.State == ConnectionState.Closed;

            try
            {
                if (closeConnection)
                    await connection.OpenAsync();

                await using var command = new MySqlCommand("GetEngagementCommunityOverview", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@startDateIn", MySqlDbType.Date).Value = model.StartDate.Date;
                command.Parameters.Add("@endDateIn", MySqlDbType.Date).Value = model.EndDate.Date;

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return NotFound("Community overview statistics were not found.");

                var result = new CommunityOverview
                {
                    TotalMembers = Convert.ToInt32(reader["TotalMembers"]),
                    NewMembers = Convert.ToInt32(reader["NewMembers"]),
                    ActiveMembers = Convert.ToInt32(reader["ActiveMembers"]),
                    MembersCommenting = Convert.ToInt32(reader["MembersCommenting"]),
                    TotalPosts = Convert.ToInt32(reader["TotalPosts"]),
                    TotalComments = Convert.ToInt32(reader["TotalComments"])
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving community overview.");

                return StatusCode(500, "An error occurred retrieving community statistics.");
            }
            finally
            {
                if (closeConnection && connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetEngagementMemberParticipation(
            [FromBody] EngagementMemberParticipationRequest request)
        {
            DateTime startDate = request.StartDate;
            DateTime endDate = request.EndDate;

            var result = new EngagementMemberParticipationResponse();

            using var connection = new MySqlConnection(
                _configuration.GetConnectionString("AivenConnection"));

            await connection.OpenAsync();

            using var command = new MySqlCommand(
                "GetEngagementMemberParticipation", connection);

            command.CommandType = CommandType.StoredProcedure;

            Console.WriteLine($"Start Date: {startDate:yyyy-MM-dd}");
            Console.WriteLine($"End Date: {endDate:yyyy-MM-dd}");
            
            command.Parameters.Add("startDateIn", MySqlDbType.Date).Value = startDate.Date;
            command.Parameters.Add("endDateIn", MySqlDbType.Date).Value = endDate.Date;

            await using (var reader = await command.ExecuteReaderAsync())
            {
                // ==========================================
                // RESULT SET 1: MEMBER PARTICIPATION SUMMARY
                // ==========================================
                if (await reader.ReadAsync())
                {
                    result.Summary = new MemberParticipationSummary
                    {
                        TotalMembers = MySqlDataReaderHelper.GetInt(reader, "TotalMembers"),
                        ActiveMembers = MySqlDataReaderHelper.GetInt(reader, "ActiveMembers"),
                        MembersWhoPosted = MySqlDataReaderHelper.GetInt(reader, "MembersWhoPosted"),
                        MembersWhoCommented = MySqlDataReaderHelper.GetInt(reader, "MembersWhoCommented"),
                        PostOnlyMembers = MySqlDataReaderHelper.GetInt(reader, "PostOnlyMembers"),
                        CommentOnlyMembers = MySqlDataReaderHelper.GetInt(reader, "CommentOnlyMembers"),
                        NeverParticipatedMembers = MySqlDataReaderHelper.GetInt(reader, "NeverParticipatedMembers"),
                        InactiveMembers = MySqlDataReaderHelper.GetInt(reader, "InactiveMembers"),
                        TotalPosts = MySqlDataReaderHelper.GetInt(reader, "TotalPosts"),
                        TotalComments = MySqlDataReaderHelper.GetInt(reader, "TotalComments")
                    };
                }

                // ==========================================
                // RESULT SET 2: MOST ACTIVE MEMBERS
                // ==========================================
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.MostActiveMembers.Add(
                            new MemberParticipationActivity
                            {
                                UserId = MySqlDataReaderHelper.GetInt(reader, "UserId"),
                                Guid = MySqlDataReaderHelper.GetString(reader, "Guid"),
                                Username = MySqlDataReaderHelper.GetString(reader, "Username"),
                                Name = MySqlDataReaderHelper.GetString(reader, "Name"),
                                ProfileImageKey = MySqlDataReaderHelper.GetNullableString(reader, "ProfileImageKey"),
                                Zipcode = MySqlDataReaderHelper.GetInt(reader, "Zipcode"),
                                City = MySqlDataReaderHelper.GetNullableString(reader, "City"),
                                State = MySqlDataReaderHelper.GetNullableString(reader, "State"),
                                PostsInRange = MySqlDataReaderHelper.GetInt(reader, "PostsInRange"),
                                CommentsInRange = MySqlDataReaderHelper.GetInt(reader, "CommentsInRange"),
                                TotalContributions = MySqlDataReaderHelper.GetInt(reader, "TotalContributions"),
                                LifetimePosts = MySqlDataReaderHelper.GetInt(reader, "LifetimePosts"),
                                LifetimeComments = MySqlDataReaderHelper.GetInt(reader, "LifetimeComments"),
                                LastActivityDate = MySqlDataReaderHelper.GetNullableDateTime(reader, "LastActivityDate")
                            });
                    }
                }

                // ==========================================
                // RESULT SET 3: PARTICIPATION PATTERNS
                // ==========================================
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.ParticipationPatterns.Add(
                            new MemberParticipationPattern
                            {
                                UserId = MySqlDataReaderHelper.GetInt(reader, "UserId"),
                                Guid = MySqlDataReaderHelper.GetString(reader, "Guid"),
                                Username = MySqlDataReaderHelper.GetString(reader, "Username"),
                                Name = MySqlDataReaderHelper.GetString(reader, "Name"),
                                ProfileImageKey = MySqlDataReaderHelper.GetNullableString(reader, "ProfileImageKey"),
                                Zipcode = MySqlDataReaderHelper.GetInt(reader, "Zipcode"),
                                City = MySqlDataReaderHelper.GetNullableString(reader, "City"),
                                State = MySqlDataReaderHelper.GetNullableString(reader, "State"),
                                ParticipationType = MySqlDataReaderHelper.GetString(reader, "ParticipationType"),
                                PostsInRange = MySqlDataReaderHelper.GetInt(reader, "PostsInRange"),
                                CommentsInRange = MySqlDataReaderHelper.GetInt(reader, "CommentsInRange"),
                                LifetimePosts = MySqlDataReaderHelper.GetInt(reader, "LifetimePosts"),
                                LifetimeComments = MySqlDataReaderHelper.GetInt(reader, "LifetimeComments"),
                                LastActivityDate = MySqlDataReaderHelper.GetNullableDateTime(reader, "LastActivityDate")
                            });
                    }
                }

                // ==========================================
                // RESULT SET 4: MEMBER RETENTION
                // ==========================================
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.MemberRetention.Add(
                            new MemberParticipationRetention
                            {
                                UserId = MySqlDataReaderHelper.GetInt(reader, "UserId"),
                                Guid = MySqlDataReaderHelper.GetString(reader, "Guid"),
                                Username = MySqlDataReaderHelper.GetString(reader, "Username"),
                                Name = MySqlDataReaderHelper.GetString(reader, "Name"),
                                ProfileImageKey = MySqlDataReaderHelper.GetNullableString(reader, "ProfileImageKey"),
                                Zipcode = MySqlDataReaderHelper.GetInt(reader, "Zipcode"),
                                City = MySqlDataReaderHelper.GetNullableString(reader, "City"),
                                State = MySqlDataReaderHelper.GetNullableString(reader, "State"),
                                DateAdded = MySqlDataReaderHelper.GetDateTime(reader, "DateAdded"),
                                RetentionType = MySqlDataReaderHelper.GetString(reader, "RetentionType"),
                                LifetimePosts = MySqlDataReaderHelper.GetInt(reader, "LifetimePosts"),
                                LifetimeComments = MySqlDataReaderHelper.GetInt(reader, "LifetimeComments"),
                                LastActivityDate = MySqlDataReaderHelper.GetNullableDateTime(reader, "LastActivityDate")
                            });
                    }
                }
            }

            // fill in the image URLS
            await _blobImageService.PopulateImageUrlsAsync(
                    result.MostActiveMembers,
                    "profile-images",
                    x => x.ProfileImageKey,
                    (x, url) => x.ProfileImageUrl = url);

            await _blobImageService.PopulateImageUrlsAsync(
                result.ParticipationPatterns,
                "profile-images",
                x => x.ProfileImageKey,
                (x, url) => x.ProfileImageUrl = url);

            await _blobImageService.PopulateImageUrlsAsync(
                result.MemberRetention,
                "profile-images",
                x => x.ProfileImageKey,
                (x, url) => x.ProfileImageUrl = url);

            return Ok(result);
        }
    }
}
