using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

[HttpGet]
[Route("[action]")]
[ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.OK)]
[ProducesResponseType((int)HttpStatusCode.NotFound)]
public async Task<ActionResult> GetDetailsForCompras(int id)
{
    var compra = await _context.Compras
        .Where(c => c.Id == id)
        .Include(c => c.Usuario)
        .Include(c => c.MetodoPago)
        .Include(c => c.CompraItems)
            .ThenInclude(ci => ci.Libro)
                .ThenInclude(l => l.Genero)
        .Include(c => c.CompraItems)
            .ThenInclude(ci => ci.Libro)
                .ThenInclude(l => l.Editorial)
        .Select(c => new ComprasDetailDTO(
            c.Id,
            c.Usuario.Name ?? "",            
            c.Usuario.Surname ?? "",          
            c.Usuario.Direccion ?? "",        
            c.Usuario.PhoneNumber ?? "",      
            c.FechaCompra,
            c.PrecioTotal,
            c.MetodoPago.GetType().Name,
            c.CodigoDescuento,
            c.CompraItems.Select(ci => new CompraItemDTO(
                ci.Libro.Id,
                ci.Libro.Titulo,
                ci.Libro.Genero.Nombre,
                ci.Libro.Editorial.Nombre,
                ci.Libro.PrecioCompra,
                ci.Cantidad
            )).ToList()
        ))
        .FirstOrDefaultAsync();

    if (compra == null)
    {
        _logger.LogError($"Error: La compra con identificador {id} no existe.");
        return NotFound();
    }

    return Ok(compra);

        }
    }
}