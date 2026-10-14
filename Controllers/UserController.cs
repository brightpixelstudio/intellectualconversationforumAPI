using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using intellectualconversationAPI.Models.Member;
using intellectualconversationAPI.Models.Statsitics;

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

        /******************************************************************************/
        // Specific member 
        /******************************************************************************/
        [HttpGet(Name = "GetPostsMember")]
        public async Task<ActionResult<IEnumerable<GetPostsMember>>> GetPostsMember(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getposts = await _context.GetPostsMember
                .FromSqlRaw("CALL GetPostsMember({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getposts);
        }

        [HttpGet(Name = "GetCommentsMember")]
        public async Task<ActionResult<IEnumerable<GetCommentsMember>>> GetCommentsMember(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getcomments = await _context.GetCommentsMember
                .FromSqlRaw("CALL GetCommentsMember({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getcomments);
        }
        
        [HttpGet(Name = "GetPostsWithMostCommentsMember")]
        public async Task<ActionResult<IEnumerable<GetPostsWithMostCommentsMember>>> GetPostsWithMostCommentsMember(string userGuid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userGuid);

            // MySQL utilizes the 'CALL' syntax
            var getposts = await _context.GetPostsWithMostCommentsMember
                .FromSqlRaw("CALL GetPostsWithMostCommentsMember({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getposts);
        }

        [HttpGet(Name = "GetProfileMember")]
        public async Task<ActionResult<IEnumerable<GetProfileMember>>> GetProfileMember(string userGuid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userGuid);

            // MySQL utilizes the 'CALL' syntax
            var getprofile = await _context.GetProfileMember
                .FromSqlRaw("CALL GetProfileMember({0})", guidIn)
                .ToListAsync();

            if (getprofile.Count == 0)
                return NotFound();

            // return results
            return Ok(getprofile);
        }

        [HttpGet(Name = "GetUserList")]
        public async Task<ActionResult<IEnumerable<GetUserList>>> GetUserList()
        {
            // MySQL utilizes the 'CALL' syntax
            var getUsers = await _context.GetUserList
                .FromSqlRaw("CALL GetUserList()")
                .ToListAsync();

            if (getUsers.Count == 0)
                return NotFound();

            // return results
            return Ok(getUsers);
        }
        

        /******************************************************************************/
        // posts, updates and deletes
        /******************************************************************************/
        [HttpPost(Name = "AddNewMember")]
        public async Task<ActionResult> AddNewMember([FromBody] FormRegistration model)
        {
            if (!ModelState.IsValid) {
                return BadRequest(new { message = "The form is not valid" });
            }
            else {

                // Define the parameter to prevent SQL Injection
                var email = new MySqlParameter("@email", model.email);
                var username = new MySqlParameter("@username", model.username);

                // verify that the email and username is not already used
                var results = await _context.IsEmailAndUsernameUsed
                    .FromSqlRaw("CALL IsEmailAndUsernameUsed({0}, {1})", email, username)
                    .ToListAsync();

                // result?
                if (results.Count > 0)
                {
                    return BadRequest(new { message = "The email or username is already being used" });
                }

                // Define the parameter to prevent SQL Injection
                var name = new MySqlParameter("@name", model.name);
                var zip = new MySqlParameter("@zip", model.zipcode);
                var password = new MySqlParameter("@password", model.password);
                var profile = new MySqlParameter("@profile", model.profile);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL InsertUser({0}, {1}, {2}, {3}, {4}, {5})", name, username, email, zip, password, profile);

                return Ok(new { message = "Member successfully inserted" });
            }
        }

        [HttpPost(Name = "UpdateMemberProfile")]
        public async Task<ActionResult> UpdateMemberProfile(string userguid, [FromBody] FormUpdateProfile model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }
            else
            {
                // Define the parameter to prevent SQL Injection
                var email = new MySqlParameter("@email", model.email);
                var username = new MySqlParameter("@username", model.username);

                // verify that the email and username is not already used
                var results = await _context.IsEmailAndUsernameUsed
                    .FromSqlRaw("CALL IsEmailAndUsernameUsedUserId({0}, {1}, {2})", email, username, userguid)
                    .ToListAsync();

                // result?
                if (results.Count > 0)
                {
                    return BadRequest(new { message = "The email or username is already being used" });
                }

                // Define the parameter to prevent SQL Injection
                var name = new MySqlParameter("@name", model.name);
                var zip = new MySqlParameter("@zip", model.zipcode);
                var password = new MySqlParameter("@password", model.password);
                var profile = new MySqlParameter("@profile", model.profile);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL UpdateUser({0}, {1}, {2}, {3}, {4}, {5}, {6})", name, username, email, zip, password, profile, userguid);

                return Ok(new { message = "Member successfully inserted" });
            }
        }
        [HttpDelete(Name = "DeleteMember")]
        public async Task<ActionResult> DeleteMember(string  userGuid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userGuid);

            // add the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL DeleteUser({0})", guidIn);

            return Ok(new { message = "Member successfully deleted" });
        }

        [HttpPost(Name = "InsertPostUserRating")]
        public async Task<ActionResult> InsertPostUserRating(string userguid, string postguid, int rating)
        {
            // Define the parameter to prevent SQL Injection
            var userguidIn = new MySqlParameter("@userguidIn", userguid);
            var postguidIn = new MySqlParameter("@postguidIn", postguid);
            var ratingIn = new MySqlParameter("@rating", rating);

            // add / update the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL InsertPostUserRating({0}, {1}, {2})", userguidIn, postguidIn, ratingIn);

            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", postguid);

            var posts = await _context.GetPost
                .FromSqlRaw("CALL GetPost({0})", guidIn)
                .ToListAsync();

            // Find the post in memory
            var post = posts.FirstOrDefault();

            if (post == null)
            {
                return NotFound(new { message = "Post not found." });
            }

            // Return the response to Angular
            return Ok(new PostRatingResponse
            {
                Postid = post.postid,
                AverageRating = Convert.ToDecimal(post.rating)
            });
        }

        [HttpPost(Name = "InsertCommentUserRating")]
        public async Task<ActionResult> InsertCommentUserRating(string userguid, string Commentguid, int rating)
        {
            // Define the parameter to prevent SQL Injection
            var userguidIn = new MySqlParameter("@userguidIn", userguid);
            var commentguidIn = new MySqlParameter("@commentguidIn", Commentguid);
            var ratingIn = new MySqlParameter("@rating", rating);

            // add / update the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL InsertCommentUserRating({0}, {1}, {2})", userguidIn, commentguidIn, ratingIn);

            // Define the parameter to prevent SQL Injection
            var CommentId = new MySqlParameter("@commentidIn", commentguidIn);

            var comments = await _context.GetComment
                .FromSqlRaw("CALL GetComment({0})", commentguidIn)
                .ToListAsync();

            // Find the post in memory
            var comment = comments.FirstOrDefault();

            if (comment == null)
            {
                return NotFound(new { message = "Comment not found." });
            }

            // Return the response to Angular
            return Ok(new CommentRatingResponse
            {
                Commentid = comment.postid,
                AverageRating = Convert.ToDecimal(comment.rating)
            });
        }
    }
}
