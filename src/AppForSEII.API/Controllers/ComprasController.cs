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

[HttpPost]
[Route("[action]")]
[ProducesResponseType(typeof(ComprasDetailDTO), (int)HttpStatusCode.Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
[ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
public async Task<ActionResult> CreateCompra(CompraForCreateDTO compraForCreate) 
{
    if (compraForCreate.CompraItems == null || compraForCreate.CompraItems.Count == 0)
        ModelState.AddModelError("CompraItems", "Error! Debes incluir al menos un libro para comprar.");

    var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == compraForCreate.CustomerUserName);
    if (user == null)
        ModelState.AddModelError("CustomerUserName", "Error! El usuario no está registrado.");

    // 1. Instanciamos el método de pago usando tus constructores vacíos
    MetodoPago metodoPagoObj = null;
    string metodoPagoString = "";

    switch (compraForCreate.MetodoPago)
    {
        case 1:
            metodoPagoObj = new Visa(); 
            metodoPagoString = "Visa";
            break;
        case 2:
            metodoPagoObj = new GooglePay(); 
            metodoPagoString = "GooglePay";
            break;
        case 3:
            metodoPagoObj = new PayPal(); 
            metodoPagoString = "PayPal";
            break;
        default:
            ModelState.AddModelError("MetodoPago", "Error! Método de pago no válido.");
            break;
    }

    if (ModelState.ErrorCount > 0)
        return BadRequest(new ValidationProblemDetails(ModelState));

    var titulosLibros = compraForCreate.CompraItems.Select(ci => ci.Titulo).ToList();
    var libros = _context.Libros.Where(l => titulosLibros.Contains(l.Titulo)).ToList();

    // 2. Uso estricto del constructor de Compra
    Compra compra = new Compra(
        0, // Id
        DateTime.Now, // FechaCompra
        0m, // PrecioTotal (decimal)
        compraForCreate.CodigoDescuento, // CodigoDescuento
        user.Id, // ApplicationUserId
        user, // Usuario
        0, // MetodoPagoId
        metodoPagoObj, // MetodoPago
        new List<CompraItem>() // CompraItems
    );

    decimal precioTotalCalculado = 0m;

    foreach (var item in compraForCreate.CompraItems) 
    {
        var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);
        
        if (libro == null || libro.Stock < item.Cantidad || libro.Stock == 0) 
        {
            ModelState.AddModelError("CompraItems", $"Error! El libro '{item.Titulo}' no tiene stock suficiente.");
        }
        else 
        {
            libro.Stock -= item.Cantidad;

            // 3. Uso estricto del constructor de CompraItem
            compra.CompraItems.Add(new CompraItem(
                item.Cantidad, 
                libro.Id, 
                libro, 
                compra.Id, 
                compra
            ));
            
            precioTotalCalculado += libro.PrecioCompra * item.Cantidad;
        }
    }

    compra.PrecioTotal = precioTotalCalculado;

    if (ModelState.ErrorCount > 0) 
    {
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    _context.Add(compra);

    try 
    {
        await _context.SaveChangesAsync();
    }
    catch (Exception ex) 
    {
        _logger.LogError(ex.Message);
        ModelState.AddModelError("Compra", "Error! Hubo un error al guardar tu compra.");
        return Conflict("Error: " + ex.Message);
    }

    //  Uso estricto del constructor de ComprasDetailDTO 
    var compraDetail = new ComprasDetailDTO(
        compra.Id, 
        compraForCreate.Nombre, 
        compraForCreate.Apellidos, 
        compraForCreate.Direccion, 
        compraForCreate.Telefono, 
        compra.FechaCompra, 
        compra.PrecioTotal, 
        metodoPagoString, // string
        compra.CodigoDescuento,
        compraForCreate.CompraItems
    );

    return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
}

    }
}