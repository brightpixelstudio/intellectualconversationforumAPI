using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Models.Posts;
using intellectualconversationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace intellectualconversationforumAPI.Controllers
{
    // Controllers
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _profileImagesContainer;
        private readonly ProfileImageService _profileImageService;

        public PostController(ILogger<UserController> logger, AppDbContext context, BlobServiceClient blobServiceClient, IConfiguration configuration, ProfileImageService profileImageService)
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

        [HttpGet(Name = "GetPostsByCategoryUserRating")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUserRating>>>
            GetPostsByCategoryUserRating(
                int? postcatagoryId,
                string? userGuid,
                float? rating,
                string? searchString)
        {
            // Define parameters
            var postcatagoryid = new MySqlParameter(
                "postcatagoryid", (object?)postcatagoryId ?? DBNull.Value);

            var guidIn = new MySqlParameter(
                "guidIn", (object?)userGuid ?? DBNull.Value);

            var ratingIn = new MySqlParameter(
                "ratingIn", (object?)rating ?? DBNull.Value);

            var searchStringIn = new MySqlParameter(
                "searchStringIn", (object?)searchString ?? DBNull.Value);

            // Retrieve posts
            var getrecords = await _context.GetPostsByCategoryUserRating
                .FromSqlRaw(
                    "CALL GetPostsByCategoryUserRating({0}, {1}, {2}, {3})",
                    postcatagoryid,
                    guidIn,
                    ratingIn,
                    searchStringIn)
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

            return Ok(getrecords);
        }
        [HttpGet(Name = "GetPostComments")]
        public async Task<ActionResult<IEnumerable<GetPostComments>>> GetPostComments(string postGuid)
        {
            var getrecords = new List<GetPostComments>();

            // Open connection from your existing EF context
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "GetPostComments";
                command.CommandType = CommandType.StoredProcedure;

                // Add the parameter cleanly
                command.Parameters.Add(new MySqlParameter("@guidIn", postGuid));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Map your object manually to prevent tracking/stale data leaks
                        var record = new GetPostComments
                        {
                            postcommentid = reader.GetInt32("postcommentid"),
                            postid = reader.GetInt32("postid"),
                            userid = reader.GetInt32("userid"),
                            commentguid = reader.GetString("commentguid"),
                            postguid = reader.GetString("postguid"),
                            userguid = reader.GetString("userguid"),
                            name = reader.GetString("name"),
                            username = reader.GetString("username"),
                            dateadded = reader.GetDateTime("dateadded"),
                            catagory = reader.GetString("catagory"),
                            rating = reader.GetFloat("rating"),
                            comment = reader.GetString("comment"),
                        };
                        getrecords.Add(record);
                    }
                } // The reader explicitly closes here, clearing MySQL's trailing status packets
            }

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        /******************************************************************************/
        // posts, updates and deletes
        /******************************************************************************/
        [HttpPost(Name = "AddPost")]
        public async Task<ActionResult> AddPost([FromBody] FormPostNew model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection
                var catagoryid = new MySqlParameter("@catagoryid", model.catagoryid);
                var guidIn = new MySqlParameter("@guidIn", model.guid);
                var post = new MySqlParameter("@post", model.post);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL InsertPost({0}, {1}, {2})", catagoryid, guidIn, post);

                return Ok(new { message = "Post successfully inserted" });
            }
        }

        [HttpPost(Name = "AddComment")]
        public async Task<ActionResult> AddComment(string postguid, [FromBody] FormCommentNew model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection                
                var postGuidIn = new MySqlParameter("@postGuidIn", postguid);
                var userGuidIn = new MySqlParameter("@userGuidIn", model.userguid);
                var comment = new MySqlParameter("@commentIn", model.comment);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL InsertComment({0}, {1}, {2})", postGuidIn, userGuidIn, comment);

                return Ok(new { message = "Comment successfully inserted" });
            }
        }
        [HttpDelete(Name = "DeletePost")]
        public async Task<ActionResult> DeletePost(int guid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", guid);

            // add the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL DeletePost({0})", guidIn);

            return Ok(new { message = "Post successfully deleted" });
        }

        [HttpDelete(Name = "DeleteComment")]
        public async Task<ActionResult> DeleteComment(int guid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", guid);

            // add the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL DeleteComment({0})", guidIn);

            return Ok(new { message = "Comment successfully deleted" });
        }

        [HttpGet(Name = "GetPost")]
        public async Task<ActionResult<IEnumerable<GetPost>>> GetPost(string postguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", postguid);

            // MySQL utilizes the 'CALL' syntax
            var getpost = await _context.GetPost
                .FromSqlRaw("CALL GetPost({0})", guidIn)
                .ToListAsync();

            if (getpost.Count == 0)
                return NotFound();

            // Generate Azure Blob Storage profile image URLs
            await _profileImageService.PopulateProfileImageUrlsAsync(
                getpost,
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            // return results
            return Ok(getpost);
        }

        [HttpGet(Name = "GetPostRatings")]
        public async Task<ActionResult<IEnumerable<GetPostRatings>>> GetPostRatings(string postguid)
        {
            // Define the parameter to prevent SQL Injection
            var postguidIn = new MySqlParameter("@postguidIn", postguid);

            // MySQL utilizes the 'CALL' syntax
            var getpostratings = await _context.GetPostRatings
                .FromSqlRaw("CALL GetPostRatings({0})", postguidIn)
                .ToListAsync();

            if (getpostratings.Count == 0)
                return NotFound();

            // return results
            return Ok(getpostratings);
        }        

        [HttpPost(Name = "UpdatePost")]
        public async Task<ActionResult> UpdatePost(string postguid, [FromBody] FormUpdatePost model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection
                var guidIn = new MySqlParameter("@guidIn", postguid);
                var catagoryIdIn = new MySqlParameter("@catagoryIdIn", model.postcatagoryid);
                var postIn = new MySqlParameter("@postIn", model.post);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL UpdatePost({0}, {1}, {2})", guidIn, catagoryIdIn, postIn);

                return Ok(new { message = "Post successfully updated" });
            }
        }

        [HttpGet(Name = "GetComment")]
        public async Task<ActionResult<IEnumerable<GetComment>>> GetComment(string commentguid)
        {
            // Define the parameter to prevent SQL Injection
            var guid = new MySqlParameter("@guid", commentguid);

            // MySQL utilizes the 'CALL' syntax
            var getcomment = await _context.GetComment
                .FromSqlRaw("CALL GetComment({0})", commentguid)
                .ToListAsync();

            if (getcomment.Count == 0)
                return NotFound();

            // return results
            return Ok(getcomment);
        }

        [HttpPost(Name = "UpdateComment")]
        public async Task<ActionResult> UpdateComment(string commentguid, [FromBody] FormUpdateComment model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection
                var commentguidIn = new MySqlParameter("@commentguidIn", commentguid);
                var userguidIn = new MySqlParameter("@userguidIn", model.userguid);
                var commentIn = new MySqlParameter("@commentIn", model.comment);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL UpdateComment({0}, {1}, {2})", commentguidIn, userguidIn, commentIn);

                return Ok(new { message = "Comment successfully updated" });
            }
        }



    }
}
