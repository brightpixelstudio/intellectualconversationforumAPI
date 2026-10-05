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

        [HttpGet(Name = "GetPostsByCategoryUser")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUser>>> GetPostsByCategoryUser(int? postcatagoryId, int? userId)
        {
            // Define the parameter to prevent SQL Injection
            var postcatagoryid = new MySqlParameter("postcatagoryid", postcatagoryId);
            var userid = new MySqlParameter("userid", userId);

            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetPostsByCategoryUser
                .FromSqlRaw("CALL GetPostsByCategoryUser({0}, {1})", postcatagoryid, userid)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetPostComments")]
        public async Task<ActionResult<IEnumerable<GetPostComments>>> GetPostComments(int postId)
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
                command.Parameters.Add(new MySqlParameter("@postidIn", postId));

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
                var userid = new MySqlParameter("@userid", model.userid);
                var post = new MySqlParameter("@post", model.post);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL InsertPost({0}, {1}, {2})", catagoryid, userid, post);

                return Ok(new { message = "Post successfully inserted" });
            }
        }

        [HttpPost(Name = "AddComment")]
        public async Task<ActionResult> AddComment(int postId, [FromBody] FormCommentNew model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection                
                var postid = new MySqlParameter("@postId", postId);
                var userid = new MySqlParameter("@userId", model.userid);
                var comment = new MySqlParameter("@comment", model.comment);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL InsertComment({0}, {1}, {2})", postid, userid, comment);

                return Ok(new { message = "Comment successfully inserted" });
            }
        }
        [HttpDelete(Name = "DeletePost")]
        public async Task<ActionResult> DeletePost(int postid)
        {
            // Define the parameter to prevent SQL Injection
            var postId = new MySqlParameter("@postidIn", postid);

            // add the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL DeletePost({0})", postId);

            return Ok(new { message = "Post successfully deleted" });
        }

        [HttpDelete(Name = "DeleteComment")]
        public async Task<ActionResult> DeleteComment(int postcommentid)
        {
            // Define the parameter to prevent SQL Injection
            var commentId = new MySqlParameter("@postcommentidIn", postcommentid);

            // add the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL DeleteComment({0})", commentId);

            return Ok(new { message = "Comment successfully deleted" });
        }

        [HttpGet(Name = "GetPost")]
        public async Task<ActionResult<IEnumerable<GetPost>>> GetPost(int postid)
        {
            // Define the parameter to prevent SQL Injection
            var postId = new MySqlParameter("@postid", postid);

            // MySQL utilizes the 'CALL' syntax
            var getpost = await _context.GetPost
                .FromSqlRaw("CALL GetPost({0})", postId)
                .ToListAsync();

            if (getpost.Count == 0)
                return NotFound();

            // return results
            return Ok(getpost);
        }

        [HttpPost(Name = "UpdatePost")]
        public async Task<ActionResult> UpdatePost(int postid, [FromBody] FormUpdatePost model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection
                var postIdIn = new MySqlParameter("@postIdIn", postid);
                var catagoryIdIn = new MySqlParameter("@catagoryIdIn", model.postcatagoryid);
                var postIn = new MySqlParameter("@postIn", model.post);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL UpdatePost({0}, {1}, {2})", postIdIn, catagoryIdIn, postIn);

                return Ok(new { message = "Post successfully updated" });
            }
        }

        [HttpGet(Name = "GetComment")]
        public async Task<ActionResult<IEnumerable<GetComment>>> GetComment(int commentid)
        {
            // Define the parameter to prevent SQL Injection
            var commentId = new MySqlParameter("@commentidIn", commentid);

            // MySQL utilizes the 'CALL' syntax
            var getcomment = await _context.GetComment
                .FromSqlRaw("CALL GetComment({0})", commentId)
                .ToListAsync();

            if (getcomment.Count == 0)
                return NotFound();

            // return results
            return Ok(getcomment);
        }

        [HttpPost(Name = "UpdateComment")]
        public async Task<ActionResult> UpdateComment(int commentid, [FromBody] FormUpdateComment model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection
                var commentId = new MySqlParameter("@commentIdIn", commentid);
                var userIdIn = new MySqlParameter("@userIdIn", model.userid);
                var commentIn = new MySqlParameter("@commentIn", model.comment);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL UpdateComment({0}, {1}, {2})", commentId, userIdIn, commentIn);

                return Ok(new { message = "Comment successfully updated" });
            }
        }



    }
}
