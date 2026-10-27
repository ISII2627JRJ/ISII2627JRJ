using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.Data; // Ajusta al namespace de tu ApplicationDbContext
using AppForSEII.API.DTOs;
using System.Net;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResenasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResenasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
[Route("GetDetailsForResenas/{id}")]
[ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.OK)]
[ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
public async Task<ActionResult<ResenaDetailDTO>> GetDetailsForResenas(int id)
{
    // Eager loading usando exactamente las propiedades de Resena.cs
    var resena = await _context.Resenas
        .Include(r => r.ApplicationUser) 
        .Include(r => r.ResenaItems)
            .ThenInclude(i => i.Libro)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (resena == null)
    {
        return NotFound($"No se encontró la reseña con el ID {id}.");
    }

    // Mapeo al DTO respetando los nombres de tu entidad
    var resenaDetailDTO = new ResenaDetailDTO(
        id: resena.Id,
        nombreCliente: resena.ApplicationUser.UserName, // Acceso a través de ApplicationUser
        fecha: resena.Fecha,                    
        items: resena.ResenaItems.Select(i => new ResenaItemDTO( 
            libroId: i.LibroId,
            tituloLibro: i.Libro.Titulo, 
            puntuacion: i.Calificacion,  // <-- Corregido para usar Calificacion
            comentario: i.Descripcion    // <-- Corregido para usar Descripcion
        )).ToList()
    );

    return Ok(resenaDetailDTO);
    }
    }
}