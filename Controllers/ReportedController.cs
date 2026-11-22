using intellectualconversationAPI.Data;
using intellectualconversationforumAPI.Models.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using MySqlConnector;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
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


        [HttpGet(Name = "GetReportedMember")]
        public async Task<ActionResult<List<FormReported>>> GetReportedMember(
                   string userguid)
        {
            var connection = (MySqlConnection)_context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand("GetReportedMember", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@reportedguidIn", userguid);
         
                var reported = new List<FormReported>();

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    reported.Add(new FormReported
                    {
                        reportedid = reader.GetInt32("reportedid"),
                        reportedstatusid = reader.GetInt32("reportedstatusid"),
                        reportedusername = reader.GetString("reportedusername"),
                        reporteduserguid = reader.GetString("reporteduserguid"),
                        statusname = reader.GetString("statusname"),
                        postid = reader.IsDBNull(reader.GetOrdinal("postid"))
                            ? null
                            : reader.GetInt32("postid"),
                        postguid = reader.IsDBNull(reader.GetOrdinal("postguid"))
                            ? null
                            : reader.GetString("postguid"),
                        postcommentid = reader.IsDBNull(reader.GetOrdinal("postcommentid"))
                            ? null
                            : reader.GetInt32("postcommentid"),
                        reportedcontent = reader.GetString("reportedcontent"),
                        reason = reader.GetString("reason"),
                        response = reader.IsDBNull(reader.GetOrdinal("response"))
                            ? null
                            : reader.GetString("response"),
                        dateadded = DateTime.SpecifyKind(
                            reader.GetDateTime("dateadded"),
                            DateTimeKind.Utc),
                        dateresponded = reader.IsDBNull(reader.GetOrdinal("dateresponded"))
                            ? null
                            : DateTime.SpecifyKind(
                                reader.GetDateTime("dateresponded"),
                                DateTimeKind.Utc
                            ),
                        respondeduserid = reader.IsDBNull(reader.GetOrdinal("respondeduserid"))
                            ? null
                            : reader.GetInt32("respondeduserid"),
                        respondedusername = reader.IsDBNull(reader.GetOrdinal("respondedusername"))
                            ? null
                            : reader.GetString("respondedusername"),
                        respondeduserguid = reader.IsDBNull(reader.GetOrdinal("respondeduserguid"))
                            ? null
                            : reader.GetString("respondeduserguid"),

                    });
                }

                return Ok(reported);
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
    }
}
