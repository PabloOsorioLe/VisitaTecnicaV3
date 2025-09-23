using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendCore.Data; 
using BackendCore.Models;

namespace BackendCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitasController : ControllerBase
    {
        private readonly BackendCoreContext _context;

        public VisitasController(BackendCoreContext context)
        {
            _context = context;
        }

        // GET: api/Visitas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Visita>>> GetVisitas()
        {
             return await _context.Visitas
            .OrderBy(v => v.Id) 
            .ToListAsync();
        }

        // GET: api/Visitas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Visita>> GetVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);

            if (visita == null)
            {
                return NotFound();
            }

            return visita;
        }

        // POST: api/Visitas
        [HttpPost]
        public async Task<ActionResult<Visita>> PostVisita(Visita visita)
        {
            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVisita", new { id = visita.Id }, visita);
        }

        // PUT: api/Visitas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVisita(int id, Visita visita)
        {
            if (id != visita.Id)
            {
                return BadRequest();
            }

            _context.Entry(visita).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VisitaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Visitas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisita(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null)
            {
                return NotFound();
            }

            _context.Visitas.Remove(visita);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VisitaExists(int id)
        {
            return _context.Visitas.Any(e => e.Id == id);
        }

    }
}
