using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Models.Statsitics;
using intellectualconversationAPI.Services;
using Azure.Storage.Blobs;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class StatisticController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _profileImagesContainer;
        private readonly ProfileImageService _profileImageService;

        public StatisticController(ILogger<UserController> logger, AppDbContext context, BlobServiceClient blobServiceClient, IConfiguration configuration, ProfileImageService profileImageService)
        {
            _logger = logger;
            _context = context;

            _blobServiceClient = blobServiceClient;
            _profileImageService = profileImageService;

            _profileImagesContainer =
                configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Profile images container is missing.");

        }

        [HttpGet(Name = "GetMostPopularCatagories")]
        public async Task<ActionResult<IEnumerable<GetMostPopularCatagories>>> GetMostPopularCatagories()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetMostPopularCatagories
                .FromSqlRaw("CALL GetMostPopularCatagories()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetMostPostsMembers")]
        public async Task<ActionResult<IEnumerable<GetMostPostsMembers>>> GetMostPostsMembers()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetMostPostsMembers
                .FromSqlRaw("CALL GetMostPostsMembers()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // Get the private profile images container
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    _profileImagesContainer);

            await _profileImageService.PopulateProfileImageUrlsAsync(
                getrecords,
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetMostPopularPostWithComments")]
        public async Task<ActionResult<IEnumerable<GetMostPopularPostWithComments>>> GetMostPopularPostWithComments()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetMostPopularPostWithComments
                .FromSqlRaw("CALL GetMostPopularPostWithComments()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // Get the private profile images container
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    _profileImagesContainer);

            await _profileImageService.PopulateProfileImageUrlsAsync(
                getrecords,
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetHighestRatingPosts")]
        public async Task<ActionResult<IEnumerable<GetHighestRatingPosts>>> GetHighestRatingPosts()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetHighestRatingPosts
                .FromSqlRaw("CALL GetHighestRatingPosts()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // Get the private profile images container
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(
                    _profileImagesContainer);

            await _profileImageService.PopulateProfileImageUrlsAsync(
                getrecords,
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            // return results
            return Ok(getrecords);
        }
    }
}
