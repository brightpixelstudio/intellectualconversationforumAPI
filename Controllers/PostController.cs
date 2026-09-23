using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Models.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace intellectualconversationforumAPI.Controllers
{
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
    }
}
