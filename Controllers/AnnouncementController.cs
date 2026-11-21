using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models.Announcements;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AnnouncementController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnnouncementController> _logger;

        public AnnouncementController(
            AppDbContext context,
            ILogger<AnnouncementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Announcement/GetAnnouncements
        [HttpGet("GetAnnouncements")]
        public async Task<IActionResult> GetAnnouncements()
        {
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "GetAnnouncements";
                command.CommandType = CommandType.StoredProcedure;

                var announcements = new List<Announcement>();

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    announcements.Add(new Announcement
                    {
                        announcementid = Convert.ToInt32(reader["announcementid"]),
                        title = reader["title"].ToString()!,
                        announcement = reader["announcement"].ToString()!,
                        dateadded = Convert.ToDateTime(reader["dateadded"]),
                        dateexpire = reader["dateexpire"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["dateexpire"])
                    });
                }

                return Ok(announcements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving announcements");
                return StatusCode(500, new { message = "Error retrieving announcements." });
            }
        }

        // POST: /Announcement/InsertAnnouncement
        [HttpPost("InsertAnnouncement")]
        public async Task<IActionResult> InsertAnnouncement(
            [FromBody] FormAnnouncement model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "InsertAnnouncement";
                command.CommandType = CommandType.StoredProcedure;

                var announcementParam = command.CreateParameter();
                announcementParam.ParameterName = "announcementIn";
                announcementParam.Value = model.announcement;
                command.Parameters.Add(announcementParam);

                var titleParam = command.CreateParameter();
                titleParam.ParameterName = "titleIn";
                titleParam.Value = model.title;
                command.Parameters.Add(titleParam);

                var expireParam = command.CreateParameter();
                expireParam.ParameterName = "dateexpireIn";
                expireParam.Value = model.dateexpire.HasValue
                    ? model.dateexpire.Value
                    : DBNull.Value;
                command.Parameters.Add(expireParam);

                var result = await command.ExecuteScalarAsync();

                int announcementId = Convert.ToInt32(result);

                return Ok(new
                {
                    success = true,
                    announcementid = announcementId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting announcement");
                return StatusCode(500, new { message = "Error inserting announcement." });
            }
        }

        // PUT: /Announcement/UpdateAnnouncement
        [HttpPut("UpdateAnnouncement")]
        public async Task<IActionResult> UpdateAnnouncement(
            [FromBody] FormAnnouncement model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.announcementid <= 0)
                return BadRequest(new { message = "Invalid announcement ID." });

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "UpdateAnnouncement";
                command.CommandType = CommandType.StoredProcedure;

                var idParam = command.CreateParameter();
                idParam.ParameterName = "@announcementidIn";
                idParam.Value = model.announcementid;
                command.Parameters.Add(idParam);

                var titleParam = command.CreateParameter();
                titleParam.ParameterName = "@titleIn";
                titleParam.Value = model.title;
                command.Parameters.Add(titleParam);

                var announcementParam = command.CreateParameter();
                announcementParam.ParameterName = "@announcementIn";
                announcementParam.Value = model.announcement;
                command.Parameters.Add(announcementParam);

                var expireParam = command.CreateParameter();
                expireParam.ParameterName = "@dateexpireIn";
                expireParam.Value = model.dateexpire.HasValue
                    ? model.dateexpire.Value
                    : DBNull.Value;
                command.Parameters.Add(expireParam);

                var result = await command.ExecuteScalarAsync();
                int rowsAffected = Convert.ToInt32(result);

                return Ok(new
                {
                    success = true,
                    rowsaffected = rowsAffected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating announcement");
                return StatusCode(500, new { message = "Error updating announcement." });
            }
        }

        // DELETE: /Announcement/ExpireAnnouncement/5
        [HttpDelete("ExpireAnnouncement/{announcementid}")]
        public async Task<IActionResult> ExpireAnnouncement(int announcementid)
        {
            if (announcementid <= 0)
                return BadRequest(new { message = "Invalid announcement ID." });

            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "ExpireAnnouncement";
                command.CommandType = CommandType.StoredProcedure;

                var idParam = command.CreateParameter();
                idParam.ParameterName = "announcementidIn";
                idParam.Value = announcementid;
                command.Parameters.Add(idParam);

                var result = await command.ExecuteScalarAsync();
                int rowsAffected = Convert.ToInt32(result);

                return Ok(new
                {
                    success = true,
                    rowsaffected = rowsAffected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring announcement");
                return StatusCode(500, new { message = "Error expiring announcement." });
            }
        }
    }
}
