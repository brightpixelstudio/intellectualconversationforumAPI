using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models.Administration;
using intellectualconversationAPI.Models.Member;
using intellectualconversationforumAPI.Models.Administration;
using IntellectualConversationForumAPI.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize]
        [HttpPost("UpdateAccountStatus")]
        public async Task<IActionResult> UpdateAccountStatus(
            [FromBody] UpdateAccountStatusRequest model)
        {
            if (model.userid <= 0)
            {
                return BadRequest(new { message = "Invalid member ID." });
            }

            int userId = model.userid;

            if (model.accountstatusid <= 0)
            {
                return BadRequest(new { message = "Invalid account status." });
            }

            if (string.IsNullOrWhiteSpace(model.reason) ||
                model.reason.Trim().Length > 1000)
            {
                return BadRequest(new
                {
                    message = "A reason is required (maximum 1000 characters)."
                });
            }

            if (string.IsNullOrWhiteSpace(model.reason) ||
                model.reason.Trim().Length < 20 )
            {
                return BadRequest(new
                {
                    message = "A reason is required to have 20 characters."
                });
            }

            // Get administrator ID from JWT.
            var adminIdClaim = User.FindFirst("userid")?.Value;

            if (!int.TryParse(adminIdClaim, out int adminId))
                return Unauthorized();

            var connection = _context.Database.GetDbConnection();
            bool openedHere = connection.State != System.Data.ConnectionState.Open;

            try
            {
                if (openedHere)
                    await connection.OpenAsync();

                // Verify current administrator role in database.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CALL GetUserTypeById(@userid);";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userid";
                    parameter.Value = adminId;
                    command.Parameters.Add(parameter);

                    var result = await command.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value ||
                        Convert.ToInt32(result) != (int)UserType.Admin)
                    {
                        return Forbid();
                    }
                }

                // Resolve target member's internal ID.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT userid FROM `user` WHERE userid = @userid LIMIT 1;";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userid";
                    parameter.Value = model.userid;
                    command.Parameters.Add(parameter);

                    var result = await command.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                        return NotFound(new { message = "Member not found." });

                    userId = Convert.ToInt32(result);
                }

                // Verify account status exists.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT COUNT(*) FROM accountstatus WHERE accountstatusid = @statusid;";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@statusid";
                    parameter.Value = model.accountstatusid;
                    command.Parameters.Add(parameter);

                    var result = await command.ExecuteScalarAsync();

                    if (Convert.ToInt32(result) == 0)
                    {
                        return BadRequest(new
                        {
                            message = "Account status does not exist."
                        });
                    }
                }

                // Update account status and insert history.
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "CALL UpdateAccountStatus(" +
                        "@userIdIn, @accountStatusIdIn, " +
                        "@changedByUserIdIn, @reasonIn);";

                    void AddParameter(string name, object value)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = name;
                        parameter.Value = value;
                        command.Parameters.Add(parameter);
                    }

                    AddParameter("@userIdIn", userId);
                    AddParameter("@accountStatusIdIn", model.accountstatusid);
                    AddParameter("@changedByUserIdIn", adminId);
                    AddParameter("@reasonIn", model.reason.Trim());

                    await command.ExecuteNonQueryAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "Member account status updated successfully."
                });
            }
            finally
            {
                if (openedHere && connection.State == System.Data.ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }
    }
}