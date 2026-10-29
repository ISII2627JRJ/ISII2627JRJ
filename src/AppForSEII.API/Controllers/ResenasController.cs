using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.Data; 
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
    
    var resena = await _context.Resenas
        .Include(r => r.ApplicationUser) 
        .Include(r => r.ResenaItems)
            .ThenInclude(i => i.Libro)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (resena == null)
    {
        return NotFound($"No se encontró la reseña con el ID {id}.");
    }

    
    var resenaDetailDTO = new ResenaDetailDTO(
        id: resena.Id,
        nombreCliente: resena.ApplicationUser.UserName, 
        fecha: resena.Fecha,                    
        items: resena.ResenaItems.Select(i => new ResenaItemDTO( 
            libroId: i.LibroId,
            tituloLibro: i.Libro.Titulo, 
            puntuacion: i.Calificacion,  
            comentario: i.Descripcion    
        )).ToList()
    );

    return Ok(resenaDetailDTO);
    }
    [HttpPost]
[Route("CreateResena")]
[ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
public async Task<ActionResult> CreateResena([FromBody] ResenaForCreateDTO resenaDTO)
{
    if (resenaDTO.Items == null || !resenaDTO.Items.Any())
    {
        ModelState.AddModelError("Items", "La reseña debe contener al menos la valoración de un libro.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    
    var usuario = await _context.Users.FindAsync(resenaDTO.ApplicationUserId);
    if (usuario == null)
    {
        ModelState.AddModelError("ApplicationUserId", "El usuario especificado no existe en el sistema.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    
    var resena = new Resena
    {
        ApplicationUserId = resenaDTO.ApplicationUserId,
        Fecha = DateTime.Now,
        ResenaItems = new List<ResenaItem>()
    };

    
    foreach (var itemDTO in resenaDTO.Items)
    {
        
        resena.ResenaItems.Add(new ResenaItem
        {
            LibroId = itemDTO.LibroId,
            Calificacion = itemDTO.Calificacion,
            Descripcion = itemDTO.Descripcion
        });
    }

    
    _context.Resenas.Add(resena);
    await _context.SaveChangesAsync();

    
    return CreatedAtAction(nameof(GetDetailsForResenas), new { id = resena.Id }, null);
}
    }
}