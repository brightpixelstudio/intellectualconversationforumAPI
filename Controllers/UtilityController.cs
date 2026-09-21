using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
