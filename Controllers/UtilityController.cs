using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

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
        
    }
}
