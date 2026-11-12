using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Models.Posts;
using intellectualconversationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using IntellectualConversationForumAPI.Enums;

namespace intellectualconversationforumAPI.Controllers
{
    // Controllers
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PostController> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _postImagesContainer;
        private readonly BlobImageService _blobImageService;
        private readonly IValidateImageService _validateImageService;

        public PostController(ILogger<PostController> logger, AppDbContext context, IConfiguration configuration, BlobImageService blobImageService, IValidateImageService validateImageService)
        {
            _logger = logger;
            _context = context;
            _blobImageService = blobImageService;
            _validateImageService = validateImageService;

            _postImagesContainer =
                configuration["AzureStorage:PostImagesContainerName"]
                ?? throw new InvalidOperationException(
                    "Post images container is missing.");
        }

        [HttpGet(Name = "GetPostsByCategoryUserRating")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUserRating>>> GetPostsByCategoryUserRating(int? postcatagoryId, string? userGuid, float? rating, string? searchString, string userloginguid)
        {
            var connection = _context.Database.GetDbConnection();

            bool shouldCloseConnection = connection.State != ConnectionState.Open;

            try
            {
                if (shouldCloseConnection)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = "CALL GetPostsByCategoryUserRating(@postcatagoryidIn, @guidIn, @ratingIn, @searchStringIn, @userguidIn)";
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new MySqlParameter(
                    "@postcatagoryidIn", (object?)postcatagoryId ?? DBNull.Value));

                command.Parameters.Add(new MySqlParameter(
                    "@guidIn", (object?)userGuid ?? DBNull.Value));

                command.Parameters.Add(new MySqlParameter(
                    "@ratingIn", (object?)rating ?? DBNull.Value));

                command.Parameters.Add(new MySqlParameter(
                    "@searchStringIn", (object?)searchString ?? DBNull.Value));

                command.Parameters.Add(new MySqlParameter(
                    "@userguidIn", (object?)userloginguid ?? DBNull.Value));

                var getrecords = new List<GetPostsByCategoryUserRating>();

                using var reader = await command.ExecuteReaderAsync();

                // Result set 1: Posts
                while (await reader.ReadAsync())
                {
                    var usertypeOrdinal = reader.GetOrdinal("usertypeid");
                    var typeOrdinal = reader.GetOrdinal("type");

                    var debugUsertypeId = reader.IsDBNull(usertypeOrdinal)
                        ? -1
                        : reader.GetInt32(usertypeOrdinal);

                    var debugType = reader.IsDBNull(typeOrdinal)
                        ? "SQL NULL"
                        : reader.GetString(typeOrdinal);


                    getrecords.Add(new GetPostsByCategoryUserRating
                    {
                        postid = reader.GetInt32(reader.GetOrdinal("postid")),
                        postguid = reader.GetString(reader.GetOrdinal("postguid")),
                        userid = reader.GetInt32(reader.GetOrdinal("userid")),
                        userguid = reader.GetString(reader.GetOrdinal("userguid")),
                        username = reader.GetString(reader.GetOrdinal("username")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        profileimagekey = reader.IsDBNull(reader.GetOrdinal("profileimagekey"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("profileimagekey")),
                        profileimageurl = null,
                        postcatagoryid = reader.GetInt32(reader.GetOrdinal("postcatagoryid")),
                        catagory = reader.GetString(reader.GetOrdinal("catagory")),
                        dateadded = reader.GetDateTime(reader.GetOrdinal("dateadded")),
                        count = reader.GetInt32(reader.GetOrdinal("count")),
                        post = reader.IsDBNull(reader.GetOrdinal("post"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("post")),
                        rating = Convert.ToSingle(reader["rating"]),
                        ratingcount = reader.GetInt32(reader.GetOrdinal("ratingcount")),
                        userrating = reader.IsDBNull(reader.GetOrdinal("userrating"))
                            ? null
                            : Convert.ToInt32(reader["userrating"]),
                        commentsclosed = Convert.ToSByte(reader["commentsclosed"]),
                        usertypeid = reader.GetInt32(reader.GetOrdinal("usertypeid")),
                        type = reader.GetString(reader.GetOrdinal("type")),
                        ispinned = reader.GetBoolean(reader.GetOrdinal("ispinned")),
                        datepinned = reader.IsDBNull(reader.GetOrdinal("datepinned"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("datepinned")),
                        pinnedbyuserid = reader.IsDBNull(reader.GetOrdinal("pinnedbyuserid"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("pinnedbyuserid")),

                    });
                }

                // Result set 2: Post images
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var postid = reader.GetInt32(reader.GetOrdinal("postid"));

                        var post = getrecords.FirstOrDefault(p => p.postid == postid);

                        if (post != null)
                        {
                            post.postimages.Add(new PostImage
                            {
                                postimageid = reader.GetInt32(reader.GetOrdinal("postimageid")),
                                postid = postid,
                                postimagekey = reader.GetString(reader.GetOrdinal("postimagekey"))
                            });
                        }
                    }
                }

                if (getrecords.Count == 0)
                    return NotFound();

                // Populate profile image URLs
                await _blobImageService.PopulateImageUrlsAsync(
                    getrecords,
                    "profile-images",
                    record => record.profileimagekey,
                    (record, url) => record.profileimageurl = url);

                // Populate post image URLs
                await _blobImageService.PopulateImageUrlsAsync(
                    getrecords.SelectMany(p => p.postimages),
                    "post-images",
                    record => record.postimagekey,
                    (record, url) => record.postimageurl = url);

                return Ok(getrecords);
            }
            finally
            {
                if (shouldCloseConnection)
                    await connection.CloseAsync();
            }
        }

        [HttpGet(Name = "GetPostsMyFeed")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUserRating>>> getPostsMyFeed(string? userGuid)
        {
            var connection = _context.Database.GetDbConnection();

            bool shouldCloseConnection = connection.State != ConnectionState.Open;

            try
            {
                if (shouldCloseConnection)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = "CALL GetPostsMyFeedBySettings(@guidIn)";
                command.CommandType = CommandType.Text;

                command.Parameters.Add(new MySqlParameter(
                    "@guidIn", (object?)userGuid ?? DBNull.Value));

                var getrecords = new List<GetPostsByCategoryUserRating>();

                using var reader = await command.ExecuteReaderAsync();

                // Result set 1: Posts
                while (await reader.ReadAsync())
                {
                    getrecords.Add(new GetPostsByCategoryUserRating
                    {
                        postid = reader.GetInt32(reader.GetOrdinal("postid")),
                        postguid = reader.GetString(reader.GetOrdinal("postguid")),
                        userid = reader.GetInt32(reader.GetOrdinal("userid")),
                        userguid = reader.GetString(reader.GetOrdinal("userguid")),
                        username = reader.GetString(reader.GetOrdinal("username")),
                        name = reader.GetString(reader.GetOrdinal("name")),

                        profileimagekey = reader.IsDBNull(reader.GetOrdinal("profileimagekey"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("profileimagekey")),

                        profileimageurl = null,

                        postcatagoryid = reader.GetInt32(reader.GetOrdinal("postcatagoryid")),
                        catagory = reader.GetString(reader.GetOrdinal("catagory")),
                        dateadded = reader.GetDateTime(reader.GetOrdinal("dateadded")),
                        count = reader.GetInt32(reader.GetOrdinal("count")),

                        post = reader.IsDBNull(reader.GetOrdinal("post"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("post")),

                        rating = Convert.ToSingle(reader["rating"]),
                        ratingcount = reader.GetInt32(reader.GetOrdinal("ratingcount")),

                        userrating = reader.IsDBNull(reader.GetOrdinal("userrating"))
                            ? null
                            : Convert.ToInt32(reader["userrating"]),

                        commentsclosed = Convert.ToSByte(reader["commentsclosed"]),
                        usertypeid = reader.GetInt32(reader.GetOrdinal("usertypeid")),
                        type = reader.GetString(reader.GetOrdinal("type")),
                        ispinned = reader.GetBoolean(reader.GetOrdinal("ispinned")),
                        datepinned = reader.IsDBNull(reader.GetOrdinal("datepinned"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("datepinned")),
                        pinnedbyuserid = reader.IsDBNull(reader.GetOrdinal("pinnedbyuserid"))
                            ? null
                            : reader.GetInt32(reader.GetOrdinal("pinnedbyuserid")),
                    });
                }

                // Result set 2: Post images
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var postid = reader.GetInt32(reader.GetOrdinal("postid"));

                        var post = getrecords.FirstOrDefault(p => p.postid == postid);

                        if (post != null)
                        {
                            post.postimages.Add(new PostImage
                            {
                                postimageid = reader.GetInt32(reader.GetOrdinal("postimageid")),
                                postid = postid,
                                postimagekey = reader.GetString(reader.GetOrdinal("postimagekey"))
                            });
                        }
                    }
                }

                if (getrecords.Count == 0)
                    return NotFound();

                // Populate profile image URLs
                await _blobImageService.PopulateImageUrlsAsync(
                    getrecords,
                    "profile-images",
                    record => record.profileimagekey,
                    (record, url) => record.profileimageurl = url);

                // Populate post image URLs
                await _blobImageService.PopulateImageUrlsAsync(
                    getrecords.SelectMany(p => p.postimages),
                    "post-images",
                    record => record.postimagekey,
                    (record, url) => record.postimageurl = url);

                return Ok(getrecords);
            }
            finally
            {
                if (shouldCloseConnection)
                    await connection.CloseAsync();
            }
        }

        [HttpGet(Name = "GetPost")]
        public async Task<ActionResult<IEnumerable<GetPost>>> GetPost( string postguid, string userguid)
        {
            var connection = _context.Database.GetDbConnection();
            bool connectionOpened = connection.State != ConnectionState.Open;

            GetPost? post = null;

            try
            {
                if (connectionOpened)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();

                command.CommandText = "CALL GetPost(@guidIn, @userguidIn)";
                command.CommandType = CommandType.Text;

                var guidParameter = command.CreateParameter();
                guidParameter.ParameterName = "@guidIn";
                guidParameter.Value = postguid;
                command.Parameters.Add(guidParameter);

                var userParameter = command.CreateParameter();
                userParameter.ParameterName = "@userguidIn";
                userParameter.Value = userguid;
                command.Parameters.Add(userParameter);

                await using var reader = await command.ExecuteReaderAsync();

                // Result set 1: Post information
                if (await reader.ReadAsync())
                {
                    post = new GetPost
                    {
                        postid = Convert.ToInt32(reader["postid"]),
                        post = reader["post"] as string,
                        userid = Convert.ToInt32(reader["userid"]),
                        userguid = reader["userguid"] as string,
                        name = reader["name"] as string,
                        username = reader["username"] as string,
                        postcatagoryid = Convert.ToInt32(reader["postcatagoryid"]),
                        catagory = reader["catagory"] as string,
                        count = Convert.ToInt32(reader["count"]),
                        rating = Convert.ToSingle(reader["rating"]),
                        ratingcount = Convert.ToInt32(reader["ratingcount"]),
                        userrating = reader["userrating"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(reader["userrating"]),
                        dateadded = Convert.ToDateTime(reader["dateadded"]),
                        commentsclosed = Convert.ToSByte(reader["commentsclosed"]),
                        profileimagekey = reader["profileimagekey"] as string
                    };
                }

                if (post == null)
                    return NotFound();

                // Result set 2: Post images
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        post.postimages.Add(new PostImage
                        {
                            postimageid = Convert.ToInt32(reader["postimageid"]),
                            postid = Convert.ToInt32(reader["postid"]),
                            postimagekey = Convert.ToString(reader["postimagekey"])!
                        });
                    }
                }
            }
            finally
            {
                if (connectionOpened)
                    await connection.CloseAsync();
            }

            // Generate profile image SAS URL
            await _blobImageService.PopulateImageUrlsAsync(
                new[] { post },
                "profile-images",
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            // Generate post image SAS URLs
            await _blobImageService.PopulateImageUrlsAsync(
                post.postimages,
                "post-images",
                record => record.postimagekey,
                (record, url) => record.postimageurl = url);

            return Ok(new[] { post });
        }

        [HttpGet(Name = "GetPostRatings")]
        public async Task<ActionResult<IEnumerable<GetRatings>>> GetPostRatings(string postguid)
        {
            if (!Guid.TryParse(postguid, out _))
                return BadRequest(new { message = "Invalid post GUID." });

            // Define the parameter to prevent SQL Injection
            var postguidIn = new MySqlParameter("@postguidIn", postguid);

            // MySQL utilizes the 'CALL' syntax
            var getpostratings = await _context.GetRatings
                .FromSqlRaw("CALL GetPostRatings({0})", postguidIn)
                .ToListAsync();

            if (getpostratings.Count == 0)
                return NotFound();

            // return results
            return Ok(getpostratings);
        }

        public async Task<ActionResult<IEnumerable<GetRatings>>> GetCommentRatings(string commentguid)
        {
            if (!Guid.TryParse(commentguid, out _))
                return BadRequest(new { message = "Invalid comment GUID." });
            
            // Define the parameter to prevent SQL Injection
            var commentguidIn = new MySqlParameter("@commentguidIn", commentguid);

            // MySQL utilizes the CALL syntax
            var getcommentratings = await _context.GetRatings
                .FromSqlRaw("CALL GetCommentRatings({0})", commentguidIn)
                .ToListAsync();

            if (getcommentratings.Count == 0)
                return NotFound();

            // Return results
            return Ok(getcommentratings);
        }


        [HttpGet(Name = "GetPostComments")]
        public async Task<ActionResult<IEnumerable<GetPostComments>>> GetPostComments(string postGuid, string userguid)
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
                command.Parameters.Add(new MySqlParameter("@userguidIn", userguid));

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
                            ratingcount = reader.IsDBNull(reader.GetOrdinal("ratingcount"))
                                ? 0
                                : reader.GetInt32(reader.GetOrdinal("ratingcount")),
                            userrating = reader.IsDBNull(reader.GetOrdinal("userrating"))
                                ? null
                                : reader.GetInt32(reader.GetOrdinal("userrating")),
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

            await _blobImageService.PopulateImageUrlsAsync(
                getrecords,
                "profile-images",
                record => record.commentprofileimagekey,
                (record, url) => record.commentprofileimageurl = url);

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetComment")]
        public async Task<ActionResult<IEnumerable<GetComment>>> GetComment(string commentguid, string userguidIn)
        {
            // Define the parameter to prevent SQL Injection
            var guid = new MySqlParameter("@guid", commentguid);
            var userguid = new MySqlParameter("@userguid", userguidIn);

            // MySQL utilizes the 'CALL' syntax
            var getcomment = await _context.GetComment
                .FromSqlRaw("CALL GetComment({0},{1})", commentguid, userguid)
                .ToListAsync();

            if (getcomment.Count == 0)
                return NotFound();

            // Generate Azure Blob Storage profile image URLs
            await _blobImageService.PopulateImageUrlsAsync(
                getcomment,
                "profile-images",
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            // return results
            return Ok(getcomment);
        }

        /******************************************************************************/
        // posts, updates and deletes
        /******************************************************************************/

        [HttpPost(Name = "AddPost")]
        public async Task<ActionResult> AddPost( [FromForm] FormPostNew model,[FromForm] List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "The form is not valid"
                });
            }

            // Validate images
            var imageValidation =
                await _validateImageService.ValidateImagesAsync(
                    images, 3, 1024 * 1024);

            if (imageValidation != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = imageValidation
                });
            }

            var connection = (MySqlConnection)
                _context.Database.GetDbConnection();

            bool openedConnection = false;

            MySqlTransaction? transaction = null;

            // Track uploaded blobs for cleanup if anything fails
            var uploadedBlobKeys = new List<string>();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                transaction = await connection.BeginTransactionAsync();

                // Insert the post
                await using var command = new MySqlCommand(
                    "InsertPost", connection, transaction);

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

                var postidOut = new MySqlParameter(
                    "@postidOut", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(postidOut);

                var postguidOut = new MySqlParameter(
                    "@postguidOut", MySqlDbType.String)
                {
                    Direction = ParameterDirection.Output,
                    Size = 36
                };                 
                command.Parameters.Add(postguidOut);

                await command.ExecuteNonQueryAsync();

                int resultCode = result.Value == DBNull.Value
                    ? -3
                    : Convert.ToInt32(result.Value);

                int newPostId = postidOut.Value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(postidOut.Value);
                
                string postguid = postguidOut.Value?.ToString();

                if (resultCode == 0)
                {
                    await transaction.RollbackAsync();

                    return Conflict(new
                    {
                        success = false,
                        message = "You have already posted this text."
                    });
                }

                if (resultCode == -1)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        success = false,
                        message = "The user could not be found."
                    });
                }

                if (resultCode != 1 || newPostId <= 0)
                {
                    throw new Exception(
                        "Unexpected result inserting post.");
                }

                // Upload images and insert their database records
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        // Upload image to Azure
                        string postimagekey =
                            await _blobImageService.UploadImageAsync(
                                image,
                                "post-images");

                        uploadedBlobKeys.Add(postimagekey);

                        // Insert image record into MySQL
                        await using var imageCommand =
                            new MySqlCommand(
                                "InsertPostImage",
                                connection,
                                transaction);

                        imageCommand.CommandType =
                            CommandType.StoredProcedure;

                        imageCommand.Parameters.Add(
                            new MySqlParameter("@guidIn", MySqlDbType.String)
                            {
                                Value = postguid
                            });

                        imageCommand.Parameters.AddWithValue(
                            "@postimagekeyIn", postimagekey);

                        var imageResult = new MySqlParameter(
                            "@resultOut", MySqlDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };

                        imageCommand.Parameters.Add(imageResult);

                        await imageCommand.ExecuteNonQueryAsync();

                        int imageResultCode =
                            imageResult.Value == DBNull.Value
                                ? -3
                                : Convert.ToInt32(imageResult.Value);

                        if (imageResultCode != 1)
                        {
                            throw new Exception(
                                $"Failed to insert post image. Result: {imageResultCode}");
                        }
                    }
                }

                // Commit post and image records
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Post successfully inserted",
                    postid = newPostId
                });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch
                    {
                        // Preserve original exception
                    }
                }

                // Remove any blobs uploaded during this request
                foreach (var blobKey in uploadedBlobKeys)
                {
                    try
                    {
                        await _blobImageService.DeleteImageAsync(
                            blobKey,
                            "post-images");
                    }
                    catch
                    {
                        // Log cleanup failure for later investigation
                    }
                }

                _logger.LogError(
                    ex,
                    "Error inserting post or uploading post images.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while inserting the post."
                });
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }

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
            var connection = (MySqlConnection)_context.Database.GetDbConnection();
            bool openedConnection = false;

            List<string> imageKeys = new();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                // 1. Retrieve all Azure blob keys before deleting the post
                await using (var command = new MySqlCommand("GetPostImageKeys", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@guidIn", guid);

                    await using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        imageKeys.Add(reader.GetString(0));
                    }
                }

                // 2. Delete the post using the existing stored procedure
                await using (var command = new MySqlCommand("DeletePost", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@guidIn", guid);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostGuid}.", guid);

                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the post."
                });
            }
            finally
            {
                if (openedConnection)
                    await connection.CloseAsync();
            }

            // 3. Delete the Azure blobs after the database deletion succeeds
            int failedDeletions = 0;

            foreach (string imageKey in imageKeys)
            {
                try
                {
                    await _blobImageService.DeleteImageAsync(imageKey,"post-images");
                }
                catch (Exception ex)
                {
                    failedDeletions++;

                    _logger.LogError(
                        ex,
                        "Failed to delete Azure post image {ImageKey} for post {PostGuid}.",
                        imageKey,
                        guid);
                }
            }

            // 4. Return the result
            if (failedDeletions > 0)
            {
                return Ok(new
                {
                    message = "Post deleted, but some images could not be removed from storage.",
                    failedImageDeletions = failedDeletions
                });
            }

            return Ok(new
            {
                message = "Post and associated images successfully deleted."
            });
        }

        [HttpPost(Name = "AddPostImage")]
        public async Task<ActionResult> AddPostImage([FromForm] string postguid, [FromForm] IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(postguid))
                return BadRequest(new { message = "Post GUID is required." });

            if (image == null || image.Length == 0)
                return BadRequest(new { message = "An image is required." });

            var imageValidation = await _validateImageService.ValidateImagesAsync(
                new List<IFormFile> { image },
                1,
                1024 * 1024);

            if (imageValidation != null)
                return BadRequest(new { message = imageValidation });

            string? uploadedBlobKey = null;

            var connection = (MySqlConnection)_context.Database.GetDbConnection();
            bool openedConnection = false;

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    uploadedBlobKey = await _blobImageService.UploadImageAsync(
                        image,
                        "post-images");

                    await using var command = new MySqlCommand(
                        "InsertPostImage",
                        connection,
                        transaction);

                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@guidIn", postguid);
                    command.Parameters.AddWithValue("@postimagekeyIn", uploadedBlobKey);

                    var resultOut = new MySqlParameter(
                        "@resultOut",
                        MySqlDbType.Int32)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };

                    command.Parameters.Add(resultOut);

                    await command.ExecuteNonQueryAsync();

                    int resultCode = Convert.ToInt32(resultOut.Value);

                    if (resultCode != 1)
                        throw new Exception($"InsertPostImage failed. Result: {resultCode}");

                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        message = "Post image successfully added."
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(uploadedBlobKey))
                {
                    try
                    {
                        await _blobImageService.DeleteImageAsync(
                            uploadedBlobKey,
                            "post-images");
                    }
                    catch (Exception cleanupException)
                    {
                        _logger.LogError(
                            cleanupException,
                            "Failed to clean up uploaded post image {BlobKey}.",
                            uploadedBlobKey);
                    }
                }

                _logger.LogError(ex, "Error adding post image.");

                return StatusCode(500, new
                {
                    message = "An error occurred while adding the post image."
                });
            }
            finally
            {
                if (openedConnection)
                    await connection.CloseAsync();
            }
        }

        [HttpDelete(Name = "DeletePostImage")]
        public async Task<ActionResult> DeletePostImage( string postguid, int postimageid)
        {
            var connection = (MySqlConnection)_context.Database.GetDbConnection();
            bool openedConnection = false;

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand(
                    "DeletePostImage", connection);

                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@guidIn", postguid);
                command.Parameters.AddWithValue("@postimageidIn", postimageid);

                var imageKeyOut = new MySqlParameter(
                    "@postimagekeyOut", MySqlDbType.VarChar, 500)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                var resultOut = new MySqlParameter(
                    "@resultOut", MySqlDbType.Int32)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.Add(imageKeyOut);
                command.Parameters.Add(resultOut);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(resultOut.Value);

                if (resultCode == -1)
                    return NotFound(new { message = "Post not found." });

                if (resultCode == -2)
                    return NotFound(new { message = "Post image not found." });

                if (resultCode != 1)
                    throw new Exception("Unable to delete post image.");

                string imageKey = imageKeyOut.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(imageKey))
                    throw new Exception("The image key was not returned.");

                await _blobImageService.DeleteImageAsync(
                    imageKey,
                    "post-images");

                return Ok(new { message = "Post image successfully deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post image.");
                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the post image."
                });
            }
            finally
            {
                if (openedConnection)
                    await connection.CloseAsync();
            }
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PinPost(string postguid)
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL PinPost({0}, {1})",
                    postguid,
                    userId);

                return Ok(new { message = "Post pinned successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning post {PostGuid}", postguid);

                return StatusCode(500, new { message = "Unable to pin post." });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UnpinPost(string postguid)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL UnpinPost({0})",
                    postguid);

                return Ok(new { message = "Post unpinned successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpinning post {PostGuid}", postguid);

                return StatusCode(500, new { message = "Unable to unpin post." });
            }
        }
    }
}
