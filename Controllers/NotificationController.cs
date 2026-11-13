using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
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

        public NotificationController(ILogger<NotificationController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        [HttpGet(Name = "GetNotifications")]
        public async Task<ActionResult<List<Notification>>> GetNotifications(
            int limit = 20,
            int offset = 0)
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (limit < 1 || limit > 100 || offset < 0)
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

                await using var command = new MySqlCommand( "GetNotifications", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@useridIn", userId);
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
                        PostId = reader.GetInt32("postid"),
                        PostCommentId = reader.GetUInt32("postcommentid"),
                        IsRead = reader.GetBoolean("isread"),
                        DateCreated = reader.GetDateTime("datecreated"),
                        DateRead = reader.IsDBNull(reader.GetOrdinal("dateread"))
                            ? null
                            : reader.GetDateTime("dateread"),
                        PostGuid = reader.GetString("postguid")
                    });
                }

                return Ok(notifications);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred retrieving notifications."
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
