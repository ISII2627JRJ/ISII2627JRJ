using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LibroController> _logger;

        public LibroController(ApplicationDbContext context, ILogger<LibroController> logger)
        {
            _context = context;
            _logger = logger;
        }


[HttpGet]
[Route("GetLibrosParaComprar")]
[ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
public async Task<ActionResult> GetLibrosParaComprar(string? genero, decimal? precioMax)
{
    // Validación lógica de los parámetros de entrada
    if (precioMax != null && precioMax < 0)
    {
        ModelState.AddModelError("precioMax", "El precio máximo filtrado no puede ser un valor negativo.");
        _logger.LogError($"{DateTime.Now} Error: precioMax ({precioMax}) es menor que 0");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    var libros = await _context.Libros
        .Include(l => l.Genero)
        .Include(l => l.Editorial)
        .Where(l => l.Stock > 0 
            && (genero == null || l.Genero.Nombre.Equals(genero))
            && (precioMax == null || l.PrecioCompra <= precioMax))
        .OrderBy(l => l.Titulo)
        .Select(l => new LibroParaComprarDTO(
            l.Id,
            l.Titulo,
            l.PrecioCompra,
            l.FechaLanzamiento,
            l.Genero.Nombre,
            l.Editorial.Nombre
        ))
        .ToListAsync();

    return Ok(libros);
}
[HttpGet]
[Route("GetLibrosParaResenar")]
[ProducesResponseType(typeof(IList<LibroParaResenarDTO>), (int)HttpStatusCode.OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
public async Task<ActionResult> GetLibrosParaResenar(string? genero, string? editorial)
{
    if (genero != null && string.IsNullOrWhiteSpace(genero))
    {
        ModelState.AddModelError("genero", "El parámetro de género no puede estar vacío si se proporciona.");
        _logger.LogError($"{DateTime.Now} Error: el filtro de género está vacío.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    var libros = await _context.Libros
        .Include(l => l.Genero)
        .Include(l => l.Editorial)
        .Where(l => (genero == null || l.Genero.Nombre.Equals(genero))
                 && (editorial == null || l.Editorial.Nombre.Equals(editorial)))
        .OrderBy(l => l.Titulo)
        .Select(l => new LibroParaResenarDTO(
            l.Id,
            l.Titulo,
            l.TipoLibro,           // Si es un Enum en la BD, quizá necesites: l.TipoLibro.ToString()
            l.Genero.Nombre,
            l.Editorial.Nombre,
            l.Autor,
            l.CalificacionMedia,   // Asegúrate de que esta propiedad exista en tu clase Libro
            l.FechaLanzamiento
        ))
        .ToListAsync();

    return Ok(libros);
}

        [HttpGet]
[Route("GetLibrosParaReponer")]
[ProducesResponseType(typeof(IList<LibroParaReponerDTO>), (int)HttpStatusCode.OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
public async Task<ActionResult> GetLibrosParaReponer(string? autor, string? genero)
{
    // Validación de parámetros
    if (autor != null && string.IsNullOrWhiteSpace(autor))
    {
        ModelState.AddModelError("autor", "El filtro de autor no puede estar vacío si se proporciona.");
        _logger.LogError($"{DateTime.Now} Error: El filtro de autor está vacío.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    if (genero != null && string.IsNullOrWhiteSpace(genero))
    {
        ModelState.AddModelError("genero", "El filtro de género no puede estar vacío si se proporciona.");
        _logger.LogError($"{DateTime.Now} Error: El filtro de género está vacío.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    var libros = await _context.Libros
        .Include(l => l.Genero)
        .Include(l => l.Editorial)
        .Where(l => l.Stock == 0
                 && (autor == null || l.Autor.Contains(autor))
                 && (genero == null || l.Genero.Nombre.Equals(genero)))
        .OrderBy(l => l.Titulo)
        .Select(l => new LibroParaReponerDTO(
            l.Id,
            l.Titulo,
            l.Genero.Nombre,
            l.FechaLanzamiento,
            l.PrecioReposicion,
            l.Autor
        ))
        .ToListAsync();

    return Ok(libros);
}
    }
}