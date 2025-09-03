using AutomationLetterWriting.Context;
using AutomationLetterWriting.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationLetterWriting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class UsersController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UsersController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("assign-org")]
        public async Task<IActionResult> AssignOrganization([FromBody] AssignUserOrgDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) return NotFound("User not found.");

            var allowedDomain = _config["Org:EmailDomain"];
            if (!string.IsNullOrWhiteSpace(dto.OrganizationEmail) && !string.IsNullOrWhiteSpace(allowedDomain))
            {
                if (!dto.OrganizationEmail.EndsWith("@" + allowedDomain, StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"OrganizationEmail must end with @{allowedDomain}");
            }

            // بررسی وجود واحد
            var unitExists = await _context.OrganizationUnits.AnyAsync(x => x.Id == dto.OrganizationUnitId);
            if (!unitExists) return BadRequest("OrganizationUnit not found.");

            user.OrganizationUnitId = dto.OrganizationUnitId;
            user.OrganizationEmail = dto.OrganizationEmail;
            user.JobTitle = dto.JobTitle;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.DisplayName,
                user.OrganizationEmail,
                user.JobTitle,
                user.OrganizationUnitId
            });
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var user = await _context.Users
                .Include(u => u.OrganizationUnit)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.DisplayName,
                    u.Email,
                    u.OrganizationEmail,
                    u.JobTitle,
                    OrganizationUnit = u.OrganizationUnit == null ? null : new { u.OrganizationUnit.Id, u.OrganizationUnit.Name }
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
        }
    }
}
