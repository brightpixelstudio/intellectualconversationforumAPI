using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using intellectualconversationAPI.Models.Member;
using intellectualconversationAPI.Models.Statsitics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _profileImagesContainer;

        public UserController(ILogger<UserController> logger, AppDbContext context, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;

            // profile images
            _blobServiceClient = blobServiceClient;

            _profileImagesContainer =
                configuration["AzureStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Profile images container is missing.");
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
        public async Task<IActionResult> GetProfileMember(string userGuid)
        {
            // Prevent SQL injection by using a parameter.
            var guidIn = new MySqlParameter("@guidIn", userGuid);

            var getprofile = await _context.GetProfileMember
                .FromSqlRaw("CALL GetProfileMember({0})", guidIn)
                .ToListAsync();

            if (getprofile.Count == 0)
                return NotFound();

            var member = getprofile[0];

            string? profileImage = null;

            // is there a profile image?
            if (!string.IsNullOrWhiteSpace(member.profileimagekey))
            {
                // Use the same container name as UploadProfileImageAsync.
                var containerClient =
                    _blobServiceClient.GetBlobContainerClient("profile-images");

                var blobClient =
                    containerClient.GetBlobClient(member.profileimagekey);

                if (await blobClient.ExistsAsync())
                {
                    var download = await blobClient.DownloadStreamingAsync();

                    using var memoryStream = new MemoryStream();

                    await download.Value.Content.CopyToAsync(memoryStream);

                    var contentType =
                        download.Value.Details.ContentType ?? "image/jpeg";

                    profileImage =
                        $"data:{contentType};base64," +
                        Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            return Ok(new
            {
                member.userid,
                member.usertypeid,
                member.type,
                member.name,
                member.username,
                member.email,
                member.zipcode,
                member.count,
                member.profile,
                member.datelastpost,
                member.datelastlogin,
                member.dateadded,
                member.blocked,
                member.profileimagekey,
                profileImage
            });
        }

        [HttpGet(Name = "GetProfileMemberImage")]
        public async Task<IActionResult> GetProfileMemberImage(string userGuid)
        {
            // Prevent SQL injection by using a parameter.
            var guidIn = new MySqlParameter("@guidIn", userGuid);

            var getprofile = await _context.GetProfileMemberImage
                .FromSqlRaw("CALL GetProfileMemberImage({0})", guidIn)
                .ToListAsync();

            if (getprofile.Count == 0)
                return NotFound();

            return Ok(new
            {
                getprofile[0].profileimagekey,
            });
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
        public async Task<ActionResult> AddNewMember([FromForm] FormRegistration model, IFormFile? profileImage)
        {
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "The form is not valid" });
                }

                // Validate the optional profile image
                if (profileImage != null && profileImage.Length > 0)
                {
                    string[] allowedTypes =
                    {
                        "image/jpeg",
                        "image/jfif",
                        "image/png",
                        "image/webp"
                    };

                    if (!allowedTypes.Contains(profileImage.ContentType, StringComparer.OrdinalIgnoreCase))
                    {
                        return BadRequest(new
                        {
                            message = "Only JPG, JFIF, PNG, and WebP images are allowed."
                        });
                    }

                    if (profileImage.Length > 250 * 1024)
                    {
                        return BadRequest(new
                        {
                            message = "The profile image cannot exceed 250k."
                        });
                    }
                }

                // Define the parameter to prevent SQL Injection
                var email = new MySqlParameter("@emailIn", model.email);
                var username = new MySqlParameter("@usernameIn", model.username);

                // verify that the email and username is not already used
                var results = await _context.IsEmailAndUsernameUsed
                .FromSqlRaw("CALL IsEmailAndUsernameUsed({0}, {1})", email, username)
                .ToListAsync();

                // result?
                if (results.Count > 0)
                {
                    return BadRequest(new { message = "The email or username is already being used" });
                }

                // upload profile image to blob storage
                string? profileImageKey = null;
                if (profileImage != null && profileImage.Length > 0)
                {
                    profileImageKey = await UploadProfileImageAsync(profileImage);
                }

                // Define the parameter to prevent SQL Injection
                var name = new MySqlParameter("@nameIn", model.name);
                var zip = new MySqlParameter("@zipIn", model.zipcode);
                var password = new MySqlParameter("@passwordIn", model.password);
                var profile = new MySqlParameter("@profileIn", (object?)model.profile ?? DBNull.Value);
                var imageKey = new MySqlParameter("@profileimagekeyiIn", (object?)profileImageKey ?? DBNull.Value);

                // add the record
                var affectedRows = _context.Database.ExecuteSqlRaw(
                    "CALL InsertUser({0}, {1}, {2}, {3}, {4}, {5}, {6})", name, username, email, zip, password, profile, imageKey);

                return Ok(new { message = "Member successfully inserted" });
            }
        }

        [HttpPost(Name = "UpdateMemberProfile")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateMemberProfile([FromQuery] string userguid, [FromForm] FormUpdateProfile model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }

            // 1. Get the current profile and existing image key.
            var currentProfile = await _context.GetProfileMemberImage
                .FromSqlRaw("CALL GetProfileMemberImage({0})", userguid)
                .ToListAsync();

            if (currentProfile.Count == 0)
            {
                return NotFound(new { message = "Member profile not found." });
            }

            string? oldProfileImageKey = currentProfile[0].profileimagekey;

            // Keep the existing image unless a new one is uploaded.
            string? profileImageKey = oldProfileImageKey;

            // 2. Validate the optional new profile image.
            var profileImage = model.profileImage;

            if (profileImage != null && profileImage.Length > 0)
            {
                string[] allowedTypes =
                {
                    "image/jpeg",
                    "image/jfif",
                    "image/png",
                    "image/webp"
                };

                if (!allowedTypes.Contains(
                    profileImage.ContentType,
                    StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "Only JPG, JFIF, PNG, and WebP images are allowed."
                    });
                }

                if (profileImage.Length > 250 * 1024)
                {
                    return BadRequest(new
                    {
                        message = "The profile image cannot exceed 250 KB."
                    });
                }
            }

            // 3. Check whether the email or username is already used.
            var email = new MySqlParameter("@email", model.email);
            var username = new MySqlParameter("@username", model.username);

            var uniquenessResults = await _context.IsEmailAndUsernameUsed
                .FromSqlRaw(
                    "CALL IsEmailAndUsernameUsedUserId({0}, {1}, {2})",
                    email, username, userguid)
                .ToListAsync();

            if (uniquenessResults.Count == 0)
            {
                return StatusCode(500, new
                {
                    message = "Unable to verify the email and username."
                });
            }

            if (uniquenessResults[0].result > 0)
            {
                return BadRequest(new
                {
                    message = "The email or username is already being used."
                });
            }

            // 4. Upload the new image only when one was supplied.
            if (profileImage != null && profileImage.Length > 0)
            {
                profileImageKey = await UploadProfileImageAsync(profileImage);
            }

            // 5. Update the member, including the retained or new image key.
            var name = new MySqlParameter("@name", model.name);
            var zip = new MySqlParameter("@zip", model.zipcode);
            var password = new MySqlParameter("@password", model.password);
            var profile = new MySqlParameter("@profile", model.profile);
            var imageKey = new MySqlParameter(
                "@profileimagekeyiIn",
                (object?)profileImageKey ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "CALL UpdateUser({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})",
                name, username, email, zip, password, profile, userguid, imageKey);

            // 6. Delete the old blob only after the database update succeeds.
            if (profileImage != null &&
                profileImage.Length > 0 &&
                !string.IsNullOrWhiteSpace(oldProfileImageKey) &&
                oldProfileImageKey != profileImageKey)
            {
                await DeleteProfileImageAsync(oldProfileImageKey);
            }

            return Ok(new { message = "Member profile successfully updated." });
        }

        [HttpDelete(Name = "DeleteMember")]
        public async Task<ActionResult> DeleteMember(string userGuid)
        {
            // 1. Get the member's profile image information.
            var currentProfile = await _context.GetProfileMemberImage
                .FromSqlRaw("CALL GetProfileMemberImage({0})", userGuid)
                .ToListAsync();

            if (currentProfile.Count == 0)
            {
                return NotFound(new
                {
                    message = "Member profile not found."
                });
            }

            // 2. Get the profile image key.
            var profileImageKey = currentProfile[0].profileimagekey;

            // 3. Delete the profile image from Azure Blob Storage.
            if (!string.IsNullOrWhiteSpace(profileImageKey))
            {
                var containerClient = _blobServiceClient
                    .GetBlobContainerClient("profile-images");

                var blobClient = containerClient
                    .GetBlobClient(profileImageKey);

                await blobClient.DeleteIfExistsAsync();
            }

            // 4. Delete the member and related database records.
            var guidIn = new MySqlParameter("@guidIn", userGuid);

            await _context.Database.ExecuteSqlRawAsync(
                "CALL DeleteUser({0})", guidIn);

            return Ok(new
            {
                message = "Member successfully deleted"
            });
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
        public async Task<ActionResult> InsertCommentUserRating(string userguid, string commentguid, int rating)
        {
            // Define the parameter to prevent SQL Injection
            var userguidIn = new MySqlParameter("@userguidIn", userguid);
            var commentguidIn = new MySqlParameter("@commentguidIn", commentguid);
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

        // Helpers
        private async Task<string> UploadProfileImageAsync(
            IFormFile profileImage)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(
                _profileImagesContainer);

            // Use a generated filename, not the user's original filename.
            var extension = profileImage.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/jfif" => ".jfif",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new InvalidOperationException(
                    "Unsupported image type.")
            };

            var blobName = $"{Guid.NewGuid():N}{extension}";
            var blobClient = containerClient.GetBlobClient(blobName);

            await using var stream = profileImage.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = profileImage.ContentType
                    }
                });

            // Return the key to save in MySQL.
            return blobName;
        }
        private async Task DeleteProfileImageAsync(string? profileImageKey)
        {
            if (string.IsNullOrWhiteSpace(profileImageKey))
                return;

            var containerClient = _blobServiceClient
                .GetBlobContainerClient("profileimages");

            var blobClient = containerClient.GetBlobClient(profileImageKey);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
