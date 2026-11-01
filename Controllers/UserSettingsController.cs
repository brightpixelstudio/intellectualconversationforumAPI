using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class UserSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserSettingsController> _logger;

        public UserSettingsController(ILogger<UserSettingsController> logger, AppDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        [HttpGet(Name = "GetUserSettings")]
        public async Task<ActionResult<IEnumerable<GetUserSettings>>> GetUserSettings(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getRecords = await _context.GetUserSettings
                .FromSqlRaw("CALL GetUserSettings({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getRecords);
        }

        [HttpGet(Name = "GetUserSettingsCatagories")]
        public async Task<ActionResult<IEnumerable<GetUserSettingsCatagories>>> GetUserSettingsCatagories(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getRecords = await _context.GetUserSettingsCatagories
                .FromSqlRaw("CALL GetUserSettingsCatagories({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getRecords);
        }

        [HttpGet(Name = "GetUserSettingsUser")]
        public async Task<ActionResult<IEnumerable<GetUserSettingsUser>>> GetUserSettingsUser(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getRecords = await _context.GetUserSettingsUser
                .FromSqlRaw("CALL GetUserSettingsUser({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getRecords);
        }

        //////////////////////////////////////////////////
        // Updates, inserts, deletes
        //////////////////////////////////////////////////
        [HttpPost(Name = "UpdateUserSettings")]
        public async Task<ActionResult> UpdateUserSettings(string userguid, [FromBody] FormUpdateUserSettings model)
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
                    "UpdateUserSettings", connection);

                command.CommandType = CommandType.StoredProcedure;

                // Define the parameter to prevent SQL Injection
                command.Parameters.AddWithValue("@userguidIn", userguid);
                command.Parameters.AddWithValue("@minpostratingIn", model.minpostrating);
                command.Parameters.AddWithValue("@showunratedIn", model.showunrated);
                command.Parameters.AddWithValue("@addnewusersIn", model.addnewusers);
                command.Parameters.AddWithValue("@addnewcatagoriesIn", model.addnewcatagories);

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    success = true,
                    message = "User Settings successfully updated"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the User Settings."
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

        [HttpPost(Name = "UpdateUserSettingsCategories")]
        public async Task<ActionResult> UpdateUserSettingsCategories(string userguid, int catagory, string action)
        {
            var connection = (MySqlConnection) _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand("UpdateUserSettingsCategories", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Define the parameter to prevent SQL Injection
                command.Parameters.AddWithValue("@guidIn", userguid);
                command.Parameters.AddWithValue("@postCatagoryIdIn", catagory);
                command.Parameters.AddWithValue("@actionIn", action);
                var result = new MySqlParameter("@resultOut", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    success = true,
                    message = "Update User Settings Categories successfully updated"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the User Settings Categories."
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

        [HttpPost(Name = "UpdateUserSettingsMembers")]
        public async Task<ActionResult> UpdateUserSettingsMembers(string userguid, string selectedUserGuid, string action)
        {
            var connection = (MySqlConnection) _context.Database.GetDbConnection();

            bool openedConnection = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    openedConnection = true;
                }

                await using var command = new MySqlCommand("UpdateUserSettingsMembers", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Define the parameter to prevent SQL Injection
                command.Parameters.AddWithValue("@guidIn", userguid);
                command.Parameters.AddWithValue("@selectedUserGuidIn", selectedUserGuid);
                command.Parameters.AddWithValue("@actionIn", action);
                var result = new MySqlParameter("@resultOut", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    success = true,
                    message = "Update User Settings Members successfully updated"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the User Settings Members."
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
