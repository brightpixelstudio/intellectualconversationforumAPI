using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetLatestLoginsMembers")]
        public async Task<ActionResult<IEnumerable<GetLatestLoginsMembers>>> GetLatestLoginsMembers()
        {
            // MySQL utilizes the 'CALL' syntax
            var getusers = await _context.GetLatestLoginsMembers
                .FromSqlRaw("CALL GetLatestLoginsMembers()")
                .ToListAsync();

            if (getusers.Count == 0)
                return NotFound();

            // return results
            return Ok(getusers);
        }

        [HttpGet(Name = "GetMostPostsMembers")]
        public async Task<ActionResult<IEnumerable<GetMostPostsMembers>>> GetMostPostsMembers()
        {
            // MySQL utilizes the 'CALL' syntax
            var getusers = await _context.GetMostPostsMembers
                .FromSqlRaw("CALL GetMostPostsMembers()")
                .ToListAsync();

            if (getusers.Count == 0)
                return NotFound();

            // return results
            return Ok(getusers);
        }

        [HttpGet(Name = "GetNewestMembers")]
        public async Task<ActionResult<IEnumerable<GetNewestMembers>>> GetNewestMembers()
        {
            // MySQL utilizes the 'CALL' syntax
            var getusers = await _context.GetNewestMembers
                .FromSqlRaw("CALL GetNewestMembers()")
                .ToListAsync();

            if (getusers.Count == 0)
                return NotFound();

            // return results
            return Ok(getusers);
        }

        [HttpGet(Name = "GetPostsMember")]
        public async Task<ActionResult<IEnumerable<GetPostsMember>>> GetPostsMember(int userid)
        {
            // Define the parameter to prevent SQL Injection
            var userId = new MySqlParameter("@userid", userid);

            // MySQL utilizes the 'CALL' syntax
            var getposts = await _context.GetPostsMember
                .FromSqlRaw("CALL GetPostsMember({0})", userId)
                .ToListAsync();

            if (getposts.Count == 0)
                return NotFound();

            // return results
            return Ok(getposts);
        }

        [HttpGet(Name = "GetPostsWithMostCommentsMember")]
        public async Task<ActionResult<IEnumerable<GetPostsWithMostCommentsMember>>> GetPostsWithMostCommentsMember(int userid)
        {
            // Define the parameter to prevent SQL Injection
            var userId = new MySqlParameter("@userid", userid);

            // MySQL utilizes the 'CALL' syntax
            var getposts = await _context.GetPostsWithMostCommentsMember
                .FromSqlRaw("CALL GetPostsWithMostCommentsMember({0})", userId)
                .ToListAsync();

            if (getposts.Count == 0)
                return NotFound();

            // return results
            return Ok(getposts);
        }

        [HttpGet(Name = "GetProfileMember")]
        public async Task<ActionResult<IEnumerable<GetProfileMember>>> GetProfileMember(int userid)
        {
            // Define the parameter to prevent SQL Injection
            var userId = new MySqlParameter("@userid", userid);

            // MySQL utilizes the 'CALL' syntax
            var getprofile = await _context.GetProfileMember
                .FromSqlRaw("CALL GetProfileMember({0})", userId)
                .ToListAsync();

            if (getprofile.Count == 0)
                return NotFound();

            // return results
            return Ok(getprofile);
        }

        [HttpGet(Name = "GetAllProfileMembers")]
        public async Task<ActionResult<IEnumerable<GetAllProfileMembers>>> GetAllProfileMembers()
        {
            // MySQL utilizes the 'CALL' syntax
            var getprofiles = await _context.GetAllProfileMembers
                .FromSqlRaw("CALL GetAllProfileMembers()")
                .ToListAsync();

            if (getprofiles.Count == 0)
                return NotFound();

            // return results
            return Ok(getprofiles);
        }
    }
}
