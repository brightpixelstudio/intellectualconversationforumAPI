using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
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

        [HttpGet(Name = "GetPostsByCategory")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategory>>> GetPostsByCategory(int postcatagoryId)
        {
            // Define the parameter to prevent SQL Injection
            var postcatagoryid = new MySqlParameter("postcatagoryid", postcatagoryId);

            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetPostsByCategory
                .FromSqlRaw("CALL GetPostsByCategory({0})", postcatagoryid)
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
                command.Parameters.Add(new MySqlParameter("@postid", postId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Map your object manually to prevent tracking/stale data leaks
                        var record = new GetPostComments
                        {
                            postid = reader.GetInt32("postid"),
                            userid = reader.GetInt32("userid"),
                            username = reader.GetString("username"),
                            dateadded = reader.GetDateTime("dateadded"),
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

            /*
            // Define the parameter to prevent SQL Injection
            var postid = new MySqlParameter("postid", postId);

            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetPostComments
                .FromSqlRaw("CALL GetPostComments({0})", postid)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
            */
        }

        /*
        [HttpGet(Name = "GetPostComments")]
        public async Task<ActionResult<IEnumerable<GetPostComments>>> GetPostComments(int postId)
        {
            // Define the parameter to prevent SQL Injection
            var postid = new MySqlParameter("postid", postId);

            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetPostComments
                .FromSqlRaw("CALL GetPostComments({0})", postid)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }
        */
    }
}
