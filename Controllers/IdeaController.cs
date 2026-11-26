using intellectualconversationAPI.Data;
using intellectualconversationAPI.Services;
using intellectualconversationforumAPI.Models.Idea;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace intellectualconversationforumAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class IdeaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IdeaController> _logger;
        private readonly BlobImageService _blobImageService;

        public IdeaController(
            AppDbContext context,
            ILogger<IdeaController> logger,
            BlobImageService blobImageService
        )
        {
            _context = context;
            _logger = logger;

            _blobImageService = blobImageService;
        }

        // POST: /Idea/InsertIdea
        [HttpPost]
        public async Task<IActionResult> InsertIdea([FromBody] FormIdeaNew model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<InsertIdeaResult>(
                        "CALL InsertIdea({0}, {1}, {2}, {3})",
                        model.UserGuid,
                        model.IdeaTypeId,
                        model.Title.Trim(),
                        model.Description.Trim())
                    .ToListAsync();

                var idea = result.FirstOrDefault();

                if (idea == null)
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unable to retrieve the submitted idea."
                    });

                return Ok(new
                {
                    success = true,
                    message = "Idea submitted successfully.",
                    ideaid = idea.ideaid,
                    guid = idea.guid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting idea.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error submitting idea."
                });
            }
        }

        // PUT: /Idea/UpdateIdea
        [HttpPut]
        public async Task<IActionResult> UpdateIdea([FromBody] FormIdeaUpdate model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<UpdateIdeaResult>(
                        "CALL UpdateIdea({0}, {1}, {2}, {3})",
                        model.IdeaGuid,
                        model.IdeaStatusId,
                        model.Response ?? string.Empty,
                        model.ResponseUserGuid
                        )
                    .ToListAsync();

                var idea = result.FirstOrDefault();

                if (idea == null)
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unable to retrieve the updated idea."
                    });

                return Ok(new
                {
                    success = true,
                    message = "Idea updated successfully.",
                    idea
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating idea.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating idea."
                });
            }
        }

        // POST: Idea/GetIdeas
        [HttpPost]
        public async Task<ActionResult<List<GetIdea>>> GetIdeas(
            [FromBody] FormGetIdeas model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ideas = await _context.Set<GetIdea>()
                    .FromSqlRaw(
                        "CALL GetIdeas({0}, {1}, {2}, {3})",
                        model.StartDate,
                        model.EndDate,
                        model.IdeaTypeId,
                        model.IdeaStatusId
                    )
                    .ToListAsync();

                // Populate profile image SAS URLs.
                await _blobImageService.PopulateImageUrlsAsync(
                    ideas,
                    "profile-images",
                    x => x.ProfileImageKey,
                    (x, url) => x.profileimageurl = url
                );

                return Ok(ideas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ideas.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving ideas.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: Idea/GetIdeaTypes
        [HttpGet]
        public async Task<ActionResult<List<GetIdeaType>>> GetIdeaTypes()
        {
            try
            {
                var types = await _context.Set<GetIdeaType>()
                    .FromSqlRaw("CALL GetIdeaTypes()")
                    .ToListAsync();

                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving idea types.");

                return StatusCode(500, "An error occurred while retrieving idea types.");
            }
        }

        // GET: Idea/GetIdeaStatuses
        [HttpGet]
        public async Task<ActionResult<List<GetIdeaStatus>>> GetIdeaStatuses()
        {
            try
            {
                var statuses = await _context.Set<GetIdeaStatus>()
                    .FromSqlRaw("CALL GetIdeaStatuses()")
                    .ToListAsync();

                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving idea statuses.");

                return StatusCode(500, "An error occurred while retrieving idea statuses.");
            }
        }
    }
}
