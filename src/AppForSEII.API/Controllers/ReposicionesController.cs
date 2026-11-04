using System;
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
    public class ReposicionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReposicionesController> _logger;

        public ReposicionesController(ApplicationDbContext context, ILogger<ReposicionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Reposiciones/GetDetailsForReposicion?id=1
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForReposicion(int id)
        {
            var reposicion = await _context.Reposiciones
                .Where(r => r.Id == id)
                .Include(r => r.Usuario)
                .Include(r => r.MetodoPago)
                .Include(r => r.ReposicionItems)
                    .ThenInclude(ri => ri.Libro)
                .Select(r => new ReposicionDetailDTO(
                    r.Id,
                    r.Usuario.Name ?? "",
                    r.Usuario.Surname ?? "",
                    r.Usuario.PhoneNumber ?? "",
                    r.FechaReposicion,
                    r.PrecioTotal,
                    r.MetodoPago.GetType().Name,
                    r.Comentario,
                    r.ReposicionItems.Select(ri => new ReposicionItemDTO(
                        ri.Libro.Id,
                        ri.Libro.Titulo,
                        ri.Libro.Autor,
                        ri.Cantidad
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (reposicion == null)
            {
                _logger.LogError($"{DateTime.Now} Error: La reposición con identificador {id} no existe.");
                return NotFound();
            }

            return Ok(reposicion);
        }
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CrearReposicion([FromBody] ReposicionForCreateDTO dto)
        {
    // 1. Validar lista de libros vacía
    if (dto.Libros == null || !dto.Libros.Any())
    {
        ModelState.AddModelError("Libros", "Debe seleccionar al menos un libro para reponer.");
        _logger.LogError($"{DateTime.Now} Error: Intento de reponer sin libros seleccionados.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    // 2. Validar cantidad mayor que 1 por cada libro (Paso 5)
    foreach (var item in dto.Libros)
    {
        if (item.Cantidad <= 1)
        {
            ModelState.AddModelError("Cantidad", $"La cantidad a reponer del libro con ID {item.LibroId} debe ser mayor que 1.");
            _logger.LogError($"{DateTime.Now} Error: Cantidad no válida ({item.Cantidad}) para el libro ID {item.LibroId}.");
        }
    }

    // 3. Validar comentario opcional (20-100 caracteres) (Paso 5)
    if (!string.IsNullOrWhiteSpace(dto.Comentario))
    {
        if (dto.Comentario.Length < 20 || dto.Comentario.Length > 100)
        {
            ModelState.AddModelError("Comentario", "El comentario debe tener entre 20 y 100 caracteres.");
            _logger.LogError($"{DateTime.Now} Error: Longitud de comentario no válida ({dto.Comentario.Length} caracteres).");
        }
    }

    // 4. Validar método de pago (Visa, GooglePay o PayPal) (Paso 5)
    var metodosValidos = new[] { "Visa", "GooglePay", "PayPal" };
    if (string.IsNullOrWhiteSpace(dto.MetodoPago) || !metodosValidos.Contains(dto.MetodoPago, StringComparer.OrdinalIgnoreCase))
    {
        ModelState.AddModelError("MetodoPago", "El método de pago debe ser Visa, GooglePay o PayPal.");
        _logger.LogError($"{DateTime.Now} Error: Método de pago no válido ({dto.MetodoPago}).");
    }

    if (!ModelState.IsValid)
    {
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    // 5. Verificar que los libros existen en la BD
    var idsLibros = dto.Libros.Select(l => l.LibroId).ToList();
    var librosEnBD = await _context.Libros
        .Where(l => idsLibros.Contains(l.Id))
        .ToDictionaryAsync(l => l.Id);

    if (librosEnBD.Count != idsLibros.Distinct().Count())
    {
        ModelState.AddModelError("Libros", "Uno o más libros seleccionados no existen.");
        _logger.LogError($"{DateTime.Now} Error: Uno o más libros seleccionados no existen.");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    // 6. Obtener método de pago
    var metodoPagoEntidad = await _context.MetodosPago
        .FirstOrDefaultAsync(m => m.GetType().Name.ToLower() == dto.MetodoPago.ToLower());

    // 7. Crear entidad Reposicion y calcular precio total
    double precioTotal = 0;
    var nuevaReposicion = new Reposicion
    {
        FechaReposicion = DateTime.Now,
        Comentario = dto.Comentario,
        MetodoPago = metodoPagoEntidad,
        ReposicionItems = new List<ReposicionItem>()
    };

    foreach (var item in dto.Libros)
    {
        var libro = librosEnBD[item.LibroId];
        precioTotal += (double)libro.PrecioReposicion * item.Cantidad;

        nuevaReposicion.ReposicionItems.Add(new ReposicionItem
        {
            LibroId = libro.Id,
            Cantidad = item.Cantidad
        });
    }

    nuevaReposicion.PrecioTotal = precioTotal;

    // 8. Guardar en Base de Datos
    _context.Reposiciones.Add(nuevaReposicion);
    await _context.SaveChangesAsync();

    // 9. Construir respuesta (Recibo)
    var resultDTO = new ReposicionDetailDTO(
        nuevaReposicion.Id,
        dto.Nombre,
        $"{dto.PrimerApellido} {dto.SegundoApellido}".Trim(),
        dto.Telefono,
        nuevaReposicion.FechaReposicion,
        nuevaReposicion.PrecioTotal,
        dto.MetodoPago,
        nuevaReposicion.Comentario,
        nuevaReposicion.ReposicionItems.Select(ri => new ReposicionItemDTO(
            ri.LibroId,
            librosEnBD[ri.LibroId].Titulo,
            librosEnBD[ri.LibroId].Autor,
            ri.Cantidad
        )).ToList()
    );

    return CreatedAtAction(nameof(GetDetailsForReposicion), new { id = nuevaReposicion.Id }, resultDTO);
}

    }
}