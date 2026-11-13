using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
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
    public class UtilityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UtilityController> _logger;

        public UtilityController(ILogger<UtilityController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetCatagoryList")]
        public async Task<ActionResult<IEnumerable<GetCatagoryList>>> GetCatagoryList()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetCatagoryList
                .FromSqlRaw("CALL GetCatagoryList()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetVerifyZipcode")]
        public async Task<ActionResult<IEnumerable<GetVerifyZipcode>>> GetVerifyZipcode(string zipcode)
        {
            // Define the parameter to prevent SQL Injection
            var zipcodeIn = new MySqlParameter("@zipcodeIn", zipcode);

            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetVerifyZipcode
                .FromSqlRaw("CALL GetVerifyZipcode({0})", zipcodeIn)
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        [HttpPost(Name = "InsertCatagory")]
        public async Task<IActionResult> InsertCatagory([FromBody] InsertCatagoryRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Catagory))
                return BadRequest("Category is required.");

            try
            {
                var connection = _context.Database.GetDbConnection();
                await _context.Database.OpenConnectionAsync();

                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = "CALL InsertCatagory(@catagoryIn);";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@catagoryIn";
                    parameter.Value = model.Catagory.Trim();

                    command.Parameters.Add(parameter);

                    using var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        return Ok(new
                        {
                            postcatagoryid = reader.GetInt32("postcatagoryid"),
                            catagory = reader.GetString("catagory")
                        });
                    }

                    return StatusCode(500, "Category was not returned.");
                }
                finally
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
            catch (MySqlException ex) when (ex.SqlState == "45000")
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the category.");
            }
        }
    }
}
