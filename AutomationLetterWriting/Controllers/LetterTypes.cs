using AutomationLetterWriting.Context;
using AutomationLetterWriting.DTOs;
using AutomationLetterWriting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationLetterWriting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LetterTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LetterTypesController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LetterTypeDto dto)
        {
            var type = new LetterType { Name = dto.Name };
            _context.LetterTypes.Add(type);
            await _context.SaveChangesAsync();
            return Ok(type);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var types = await _context.LetterTypes.AsNoTracking().ToListAsync();
            return Ok(types);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LetterTypeDto dto)
        {
            var type = await _context.LetterTypes.FindAsync(id);
            if (type == null) return NotFound();

            type.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok(type);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _context.LetterTypes.FindAsync(id);
            if (type == null) return NotFound();

            _context.LetterTypes.Remove(type);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }
    }
}
