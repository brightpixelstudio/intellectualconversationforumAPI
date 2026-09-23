using Microsoft.AspNetCore.Mvc;
using intellectualconversationAPI.Data;
using Microsoft.EntityFrameworkCore;
using intellectualconversationAPI.Models.Statsitics;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")] // Adds the method name to the URL path
    public class StatisticController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public StatisticController(ILogger<UserController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetMostPopularCatagories")]
        public async Task<ActionResult<IEnumerable<GetMostPopularCatagories>>> GetMostPopularCatagories()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetMostPopularCatagories
                .FromSqlRaw("CALL GetMostPopularCatagories()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetMostPostsMembers")]
        public async Task<ActionResult<IEnumerable<GetMostPostsMembers>>> GetMostPostsMembers()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetMostPostsMembers
                .FromSqlRaw("CALL GetMostPostsMembers()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }

        [HttpGet(Name = "GetMostPopularPostWithComments")]
        public async Task<ActionResult<IEnumerable<GetMostPopularPostWithComments>>> GetMostPopularPostWithComments()
        {
            // MySQL utilizes the 'CALL' syntax
            var getrecords = await _context.GetMostPopularPostWithComments
                .FromSqlRaw("CALL GetMostPopularPostWithComments()")
                .ToListAsync();

            if (getrecords.Count == 0)
                return NotFound();

            // return results
            return Ok(getrecords);
        }
    }
}
