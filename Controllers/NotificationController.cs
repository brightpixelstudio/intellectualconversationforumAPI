using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Services;
using intellectualconversationforumAPI.Models.Notification;
using intellectualconversationforumAPI.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System.Reflection.Emit;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationController> _logger;
        private readonly BlobImageService _blobImageService;

        public NotificationController(ILogger<NotificationController> logger, AppDbContext context, BlobImageService blobImageService)
        {
            _logger = logger;
            _context = context;
            _blobImageService = blobImageService;
        }

        [Authorize]
        [HttpGet(Name = "GetNotificationsMember")]
        public async Task<ActionResult<List<Notification>>> GetNotificationsMember(
            int userid,
            int limit = 20,
            int offset = 0)
        {
            if (limit < 1 || limit > 200 || offset < 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid pagination parameters."
                });
            }

            var connection = (MySqlConnection) _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand("GetNotificationsMember", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@useridIn", userid);
                command.Parameters.AddWithValue("@limitIn", limit);
                command.Parameters.AddWithValue("@offsetIn", offset);

                var notifications = new List<Notification>();

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    notifications.Add(new Notification
                    {
                        NotificationId = reader.GetInt64("notificationid"),
                        RecipientUserId = reader.GetInt32("recipientuserid"),
                        ActorUserId = reader.GetInt32("actoruserid"),
                        ActorUsername = reader.GetString("actorusername"),
                        ActorName = reader.GetString("actorname"),
                        ActorGuid = reader.GetString("actorguid"),
                        PostShortText = reader.IsDBNull(reader.GetOrdinal("postshorttext"))
                            ? null
                            : reader.GetString("postshorttext"),
                        PostId = reader.IsDBNull(reader.GetOrdinal("postid"))
                            ? null
                            : reader.GetInt32("postid"),
                        PostCommentId = reader.IsDBNull(reader.GetOrdinal("postcommentid"))
                            ? null
                            : reader.GetInt32("postcommentid"),
                        ReportedId = reader.IsDBNull(reader.GetOrdinal("reportedid"))
                            ? null
                            : reader.GetInt32("reportedid"),
                        DateCreated = DateTime.SpecifyKind(
                            reader.GetDateTime("datecreated"),
                            DateTimeKind.Utc
                        ),
                        DateRead = reader.IsDBNull(reader.GetOrdinal("dateread"))
                            ? null
                            : DateTime.SpecifyKind(
                                reader.GetDateTime("dateread"),
                                DateTimeKind.Utc
                            ),
                        RespondedUserId = reader.IsDBNull(reader.GetOrdinal("respondeduserid"))
                            ? null
                            : reader.GetInt32("respondeduserid"),                        
                        ProfileImageKey = reader.IsDBNull(reader.GetOrdinal("profileimagekey"))
                            ? null
                            : reader.GetString("profileimagekey"),
                        PostGuid = reader.GetString("postguid")
                    });
                }

                await _blobImageService.PopulateImageUrlsAsync(
                    notifications,
                    "profile-images",
                    n => n.ProfileImageKey,
                    (n, url) => n.ProfileImageUrl = url
                );

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.ToString()
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
        [HttpGet(Name = "GetUnreadNotificationCount")]
        public async Task<ActionResult<int>> GetUnreadNotificationCount()
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var connection = (MySqlConnection) _context.Database.GetDbConnection();
            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand("GetUnreadNotificationCount", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@useridIn", userId);

                var result = await command.ExecuteScalarAsync();

                int unreadCount = Convert.ToInt32(result);

                return Ok(unreadCount);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred retrieving the unread notification count."
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
        [HttpPut(Name = "MarkNotificationRead")]
        public async Task<ActionResult> MarkNotificationRead(long notificationid)
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var connection = (MySqlConnection) _context.Database.GetDbConnection();
            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand("MarkNotificationRead", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@notificationidIn", notificationid);
                command.Parameters.AddWithValue("@useridIn", userId);

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred marking the notification as read."
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
        [HttpPut(Name = "MarkAllNotificationsRead")]
        public async Task<ActionResult> MarkAllNotificationsRead()
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var connection = (MySqlConnection) _context.Database.GetDbConnection();
            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand(
                    "MarkAllNotificationsRead", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@useridIn", userId);

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    success = true,
                    message = "All notifications marked as read."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred marking notifications as read."
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
