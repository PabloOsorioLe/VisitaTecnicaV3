using BackendCore.Data;
using BackendCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReunionesController : ControllerBase
    {
        private readonly BackendCoreContext _context;

        public ReunionesController(BackendCoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reunion>>> GetReuniones()
        {
            return await _context.Reuniones.ToListAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReunion(int id, Reunion reunion)
        {
            if (id != reunion.Id)
                return BadRequest();

            _context.Entry(reunion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReunion(int id)
        {
            var reunion = await _context.Reuniones.FindAsync(id);
            if (reunion == null)
                return NotFound();

            _context.Reuniones.Remove(reunion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Reunion>> PostReunion(Reunion reunion)
        {
            _context.Reuniones.Add(reunion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReuniones), new { id = reunion.Id }, reunion);
        }
    }
}
