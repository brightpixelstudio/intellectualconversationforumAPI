using intellectualconversationAPI.Data;
using intellectualconversationforumAPI.Models.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
   public class ReportedController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnnouncementController> _logger;

        public ReportedController(
            AppDbContext context,
            ILogger<AnnouncementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: /Report/InsertReported
        [HttpPost("InsertReported")]
        public async Task<IActionResult> InsertReported(
            [FromBody] FormReported model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL InsertReport({0}, {1}, {2}, {3})",
                    model.reporteduserguid,
                    model.postid,
                    model.postcommentid,
                    model.reason
                );

                return Ok(new
                {
                    success = true,
                    message = "Report submitted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error submitting report.",
                    error = ex.Message
                });
            }
        }

        // PUT: /Report/UpdateReported
        /*
        [HttpPut("UpdateReported")]
        public async Task<IActionResult> UpdateReported(
            [FromBody] FormReported model)
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
    */
    }
}
