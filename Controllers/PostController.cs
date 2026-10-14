using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Models.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace intellectualconversationforumAPI.Controllers
{
    // Controllers
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public PostController(ILogger<UserController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetPostsByCategoryUserRating")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUserRating>>> GetPostsByCategoryUserRating(int? postcatagoryId, string? userGuid, float? rating, string? searchString)
        {
            // Define the parameter to prevent SQL Injection
            var postcatagoryid = new MySqlParameter("postcatagoryid", postcatagoryId);
            var guidIn = new MySqlParameter("guidIn", userGuid);
            var ratingIn = new MySqlParameter("ratingIn", rating);
            var searchStringIn = new MySqlParameter("searchStringIn", searchString);

            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetPostsByCategoryUserRating
                .FromSqlRaw("CALL GetPostsByCategoryUserRating({0}, {1}, {2}, {3})", postcatagoryid, guidIn, ratingIn, searchString)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
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
        public async Task<ActionResult> AddComment(int postguid, [FromBody] FormCommentNew model)
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
                var comment = new MySqlParameter("@comment", model.comment);

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

            // return results
            return Ok(getpost);
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
