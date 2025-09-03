using AutomationLetterWriting.Context;
using AutomationLetterWriting.DTOs;
using AutomationLetterWriting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationLetterWriting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationUnitsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public OrganizationUnitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrgUnitDto dto)
        {
            var unit = new OrganizationUnit { Name = dto.Name, ParentId = dto.ParentId };
            _context.OrganizationUnits.Add(unit);
            await _context.SaveChangesAsync();
            return Ok(new { unit.Id, unit.Name, unit.ParentId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var units = await _context.OrganizationUnits
                .AsNoTracking()
                .Select(u => new { u.Id, u.Name, u.ParentId })
                .ToListAsync();

            return Ok(units);
        }
    }
}
