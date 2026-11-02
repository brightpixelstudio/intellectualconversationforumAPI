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

        [HttpGet(Name = "GetUserSettingsCatagoriesMyFeed")]
        public async Task<ActionResult<IEnumerable<GetUserSettingsCatagories>>> GetUserSettingsCatagoriesMyFeed(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getRecords = await _context.GetUserSettingsCatagories
                .FromSqlRaw("CALL GetUserSettingsCatagoriesMyFeed({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getRecords);
        }

        [HttpGet(Name = "GetUserSettingsUserMyFeed")]
        public async Task<ActionResult<IEnumerable<GetUserSettingsUser>>> GetUserSettingsUserMyFeed(string userguid)
        {
            // Define the parameter to prevent SQL Injection
            var guidIn = new MySqlParameter("@guidIn", userguid);

            // MySQL utilizes the 'CALL' syntax
            var getRecords = await _context.GetUserSettingsUser
                .FromSqlRaw("CALL GetUserSettingsUserMyFeed({0})", guidIn)
                .ToListAsync();

            // return results
            return Ok(getRecords);
        }

        //////////////////////////////////////////////////
        // Updates, inserts, deletes
        //////////////////////////////////////////////////
        [HttpPost(Name = "UpdateUserSettings")]
        public async Task<ActionResult> UpdateUserSettings(string userguid, int showUnrated, float minPostRating, int addNewUsers, int addNewCatagories)
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

                await using var command = new MySqlCommand(
                    "UpdateUserSettings", connection);
                  
                command.CommandType = CommandType.StoredProcedure;

                // Define the parameter to prevent SQL Injection
                command.Parameters.AddWithValue("@userguidIn", userguid);
                command.Parameters.AddWithValue("@minpostratingIn", minPostRating);
                command.Parameters.AddWithValue("@showunratedIn", showUnrated);
                command.Parameters.AddWithValue("@addnewusersIn", addNewUsers);
                command.Parameters.AddWithValue("@addnewcatagoriesIn", addNewCatagories);

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

        [HttpPost]
        public async Task<ActionResult> updateUserSettingsCatagories(
            [FromBody] UpdateUserSettingsCategoriesRequest request)
        { 
            // validate
            if (string.IsNullOrWhiteSpace(request.guid) ||
                request.categories == null ||
                request.categories.Any(c =>
                    c.postcatagoryid <= 0 ||
                    (c.selected != 0 && c.selected != 1)))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid category settings submitted."
                });
            }

            var connection = (MySqlConnection)_context.Database.GetDbConnection();
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
                
                // parameters
                command.Parameters.AddWithValue("@guidIn", request.guid);
                command.Parameters.Add("@categoriesIn", MySqlDbType.JSON).Value = System.Text.Json.JsonSerializer.Serialize(request.categories);
                var result = new MySqlParameter("@resultOut", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(result.Value);

                if (resultCode == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                if (resultCode != 1)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while updating categories."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User category settings successfully updated."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating categories."
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
        public async Task<ActionResult> updateUserSettingsMembers(
            [FromBody] UpdateUserSettingsUsersRequest request)
        {
            // validate
            if (string.IsNullOrWhiteSpace(request.guid) ||
                request.users == null ||
                request.users.Any(c =>
                    c.userid <= 0 ||
                    (c.selected != 0 && c.selected != 1)))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid user settings submitted."
                });
            }

            var connection = (MySqlConnection)_context.Database.GetDbConnection();
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

                // parameters
                command.Parameters.AddWithValue("@guidIn", request.guid);
                command.Parameters.Add("@usersIn", MySqlDbType.JSON).Value = System.Text.Json.JsonSerializer.Serialize(request.users);
                var result = new MySqlParameter("@resultOut", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(result);

                await command.ExecuteNonQueryAsync();

                int resultCode = Convert.ToInt32(result.Value);

                if (resultCode == -1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "User not found."
                    });
                }

                if (resultCode != 1)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while updating users."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "User settings successfully updated."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating users."
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
