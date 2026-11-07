using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Models.Posts;
using intellectualconversationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using Azure.Storage.Blobs;

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
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUserRating>>>
            GetPostsByCategoryUserRating(
                int? postcatagoryId,
                string? userGuid,
                float? rating,
                string? searchString,
                string userloginguid)
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

            var userGuidIn = new MySqlParameter(
                "userGuidIn", (object?)userloginguid ?? DBNull.Value);

            // Retrieve posts
            var getrecords = await _context.GetPostsByCategoryUserRating
                .FromSqlRaw(
                    "CALL GetPostsByCategoryUserRating({0}, {1}, {2}, {3}, {4} )",
                    postcatagoryid,
                    guidIn,
                    ratingIn,
                    searchStringIn, 
                    userGuidIn)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            await _blobImageService.PopulateImageUrlsAsync(
                getrecords,
                "profile-images",
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            return Ok(getrecords);
        }

        [HttpGet(Name = "GetPostsMyFeed")]
        public async Task<ActionResult<IEnumerable<GetPostsByCategoryUserRating>>>
            getPostsMyFeed(string? userGuid)
        {
            var guidIn = new MySqlParameter( "guidIn", (object?)userGuid ?? DBNull.Value);

            // Retrieve posts
            var getrecords = await _context.GetPostsByCategoryUserRating
                .FromSqlRaw(
                    "CALL GetPostsMyFeedBySettings({0})", guidIn)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            await _blobImageService.PopulateImageUrlsAsync(
                getrecords,
                "profile-images",
                record => record.profileimagekey,
                (record, url) => record.profileimageurl = url);

            return Ok(getrecords);
        }


        [HttpGet(Name = "GetPost")]
        public async Task<ActionResult<IEnumerable<GetPost>>> GetPost(
            string postguid,
            string userguid)
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
        public async Task<ActionResult> AddPost(
            [FromForm] FormPostNew model,
            [FromForm] List<IFormFile>? images)
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
    }
}
