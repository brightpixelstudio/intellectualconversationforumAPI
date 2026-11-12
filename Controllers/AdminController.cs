using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Data;
using IntellectualConversationForumAPI.Enums;
using intellectualconversationAPI.Models.Administration;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            AppDbContext context,
            ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("VerifyAdminAccess")]
        public async Task<IActionResult> VerifyAdminAccess()
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            try
            {
                int userTypeId = 0;

                await using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "GetUserTypeById";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "userIdIn";
                    parameter.Value = userId;
                    command.Parameters.Add(parameter);

                    if (command.Connection!.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                        userTypeId = Convert.ToInt32(result);
                }

                if (userTypeId != (int)UserType.Admin)
                    return Forbid();

                return Ok(new { isAdmin = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error verifying administrator access for user {UserId}",
                    userId);

                return StatusCode(500,
                    new { message = "Unable to verify administrator access." });
            }
        }

        [HttpGet("SearchMembers")]
        public async Task<IActionResult> SearchMembers([FromQuery] string searchText)
        {
            var userIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            searchText = searchText?.Trim() ?? "";

            if (searchText.Length < 3 || searchText.Length > 100)
                return BadRequest(new { message = "Search must contain between 3 and 100 characters." });

            try
            {
                var connection = _context.Database.GetDbConnection();

                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                // Verify the user's current administrator role.
                int userTypeId = 0;

                await using (var adminCommand = connection.CreateCommand())
                {
                    adminCommand.CommandText = "GetUserTypeById";
                    adminCommand.CommandType = System.Data.CommandType.StoredProcedure;

                    var parameter = adminCommand.CreateParameter();
                    parameter.ParameterName = "userIdIn";
                    parameter.Value = userId;
                    adminCommand.Parameters.Add(parameter);

                    var result = await adminCommand.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                        userTypeId = Convert.ToInt32(result);
                }

                if (userTypeId != (int)UserType.Admin)
                    return Forbid();

                // Execute the member search stored procedure.
                var members = new List<MemberSearchResult>();

                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = "AdminSearchMembers";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "searchTextIn";
                    parameter.Value = searchText;
                    command.Parameters.Add(parameter);

                    await using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        members.Add(new MemberSearchResult
                        {
                            userguid = reader["userguid"].ToString()!,
                            name = reader["name"].ToString()!,
                            username = reader["username"].ToString()!,
                            email = reader["email"].ToString()!,
                            usertypeid = Convert.ToInt32(reader["usertypeid"]),
                            type = reader["type"].ToString()!
                        });
                    }
                }

                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error searching members for administrator {UserId}",
                    userId);

                return StatusCode(500,
                    new { message = "Unable to search members." });
            }
        }
    }
}