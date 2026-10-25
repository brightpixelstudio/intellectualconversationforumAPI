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

        public PostController(ILogger<UserController> logger, AppDbContext context, IConfiguration configuration, ProfileImageService profileImageService)
        {
            _logger = logger;
            _context = context;
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
                            commentprofileimagekey =
                                reader.IsDBNull(reader.GetOrdinal("commentprofileimagekey"))
                                    ? null
                                    : reader.GetString("commentprofileimagekey"),
                            commentprofileimageurl = ""
                        };
                        getrecords.Add(record);
                    }
                } // The reader explicitly closes here, clearing MySQL's trailing status packets
            }

            if (getrecords.Count == 0)
                return NotFound();

            await _profileImageService.PopulateProfileImageUrlsAsync(
                getrecords,
                record => record.commentprofileimagekey,
                (record, url) => record.commentprofileimageurl = url);

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
                return BadRequest(new
                {
                    success = false,
                    message = "The form is not valid"
                });
            }

            var connection = (MySqlConnection)
                _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand(
                    "InsertPost", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "@catagoryIdIn", model.catagoryid);

                command.Parameters.AddWithValue(
                    "@guidIn", model.guid);

                command.Parameters.AddWithValue(
                    "@postIn", model.post);

                var result = new MySqlParameter(
                    "@resultIn", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(result.Value);

                if (resultCode == 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "You have already posted this text."
                    });
                }

                if (resultCode == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "The user could not be found."
                    });
                }

                if (resultCode != 1)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unexpected result inserting post."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Post successfully inserted"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while inserting the post."
                });
            }
            finally
            {
                if (openedConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }

        [HttpPost(Name = "AddComment")]
        public async Task<ActionResult> AddComment(string postguid, [FromBody] FormCommentNew model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "The form is not valid" });
            }

            var connection = (MySqlConnection)
            _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand(
                    "InsertComment", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "@postGuidIn", postguid);

                command.Parameters.AddWithValue(
                    "@userGuidIn", model.userguid);

                command.Parameters.AddWithValue(
                    "@commentIn", model.comment);

                var result = new MySqlParameter(
                    "@resultIn", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(result.Value);

                if (resultCode == 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "You have already posted this comment."
                    });
                }

                if (resultCode == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "The user could not be found."
                    });
                }

                if (resultCode != 1)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unexpected result inserting comment."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Comment successfully inserted"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while inserting the comment."
                });
            }
            finally
            {
                if (openedConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }
        [HttpDelete(Name = "DeletePost")]
        public async Task<ActionResult> DeletePost(string guid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", guid);

            // add the record
            var affectedRows = _context.Database.ExecuteSqlRaw(
                "CALL DeletePost({0})", guidIn);

            return Ok(new { message = "Post successfully deleted" });
        }

        [HttpDelete(Name = "DeleteComment")]
        public async Task<ActionResult> DeleteComment(string guid)
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
                return BadRequest(new
                {
                    success = false,
                    message = "The form is not valid"
                });
            }

            var connection = (MySqlConnection)
                _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand(
                    "UpdatePost", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "@guidIn", postguid);

                command.Parameters.AddWithValue(
                    "@catagoryIdIn", model.postcatagoryid);

                command.Parameters.AddWithValue(
                    "@postIn", model.post);

                var result = new MySqlParameter(
                                   "@resultIn", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(result.Value);

                if (resultCode == 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "You have already posted this comment text."
                    });
                }

                if (resultCode == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "The user could not be found."
                    });
                }

                if (resultCode != 1)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unexpected result updating post."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Post successfully updated"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the post."
                });
            }
            finally
            {
                if (openedConnection)
                {
                    await connection.CloseAsync();
                }
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

            // Generate Azure Blob Storage profile image URLs
            await _profileImageService.PopulateProfileImageUrlsAsync(
                getcomment,
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);
            
            // return results
            return Ok(getcomment);
        }

        [HttpPost(Name = "UpdateComment")]
        public async Task<ActionResult> UpdateComment(string commentguid, [FromBody] FormUpdateComment model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "The form is not valid"
                });
            }

            var connection = (MySqlConnection)
                _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand(
                    "UpdateComment", connection);

                command.CommandType = CommandType.StoredProcedure;

                // Define the parameter to prevent SQL Injection
                command.Parameters.AddWithValue("@commentguidIn", commentguid);
                command.Parameters.AddWithValue("@userguidIn", model.userguid);
                command.Parameters.AddWithValue("@commentIn", model.comment);

                var result = new MySqlParameter("@resultIn", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(result.Value);

                if (resultCode == 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "You have already posted this comment text."
                    });
                }

                if (resultCode == 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "You have already posted this comment text."
                    });
                }

                if (resultCode == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "The user could not be found."
                    });
                }

                if (resultCode != 1)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unexpected result updating comment."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Comment successfully updated"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the comment."
                });
            }
            finally
            {
                if (openedConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
