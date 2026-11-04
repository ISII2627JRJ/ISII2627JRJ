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
                .Select(c => new ComprasDetailDTO(
                    c.Id,
                    c.Usuario.Name,
                    c.Usuario.Surname,
                    c.Usuario.Direccion,
                    c.Usuario.PhoneNumber,
                    c.FechaCompra,
                    c.PrecioTotal,
                    c.MetodoPago is Visa ? "Visa" : (c.MetodoPago is GooglePay ? "GooglePay" : "PayPal"),
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
                _logger.LogError($"{DateTime.Now} Error: La compra con id {id} no existe.");
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
            // 1. Debe haber al menos un libro
            if (compraForCreate.CompraItems == null || compraForCreate.CompraItems.Count == 0)
                ModelState.AddModelError("CompraItems", "Error! Debes incluir al menos un libro para comprar.");

            // 2. El usuario debe existir
            var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == compraForCreate.CustomerUserName);
            if (user == null)
                ModelState.AddModelError("CustomerUserName", "Error! El usuario no está registrado.");

            // 3. Buscamos el método de pago que ya existe en la BD (1=Visa, 2=GooglePay, 3=PayPal)
            MetodoPago metodoPago = null;
            string metodoPagoTexto = "";
            if (compraForCreate.MetodoPago == 1)
            {
                metodoPago = _context.MetodosPago.OfType<Visa>().FirstOrDefault();
                metodoPagoTexto = "Visa";
            }
            else if (compraForCreate.MetodoPago == 2)
            {
                metodoPago = _context.MetodosPago.OfType<GooglePay>().FirstOrDefault();
                metodoPagoTexto = "GooglePay";
            }
            else if (compraForCreate.MetodoPago == 3)
            {
                metodoPago = _context.MetodosPago.OfType<PayPal>().FirstOrDefault();
                metodoPagoTexto = "PayPal";
            }

            if (metodoPago == null)
                ModelState.AddModelError("MetodoPago", "Error! Método de pago no válido.");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // 4. Guardamos en el usuario los datos que ha escrito en el formulario
            user.Name = compraForCreate.Nombre;
            user.Surname = compraForCreate.Apellidos;
            user.Direccion = compraForCreate.Direccion;
            user.PhoneNumber = compraForCreate.Telefono;

            // 5. Creamos la compra (el precio total lo calculamos después)
            Compra compra = new Compra(0, DateTime.Now, 0m, compraForCreate.CodigoDescuento,
                                       user.Id, user, metodoPago.Id, metodoPago, new List<CompraItem>());

            // Lista con los libros que devolveremos en la respuesta
            List<CompraItemDTO> librosComprados = new List<CompraItemDTO>();

            // 6. Recorremos cada libro del carrito
            foreach (var item in compraForCreate.CompraItems)
            {
                // Buscamos el libro por su Id, con su género y editorial
                var libro = _context.Libros
                    .Include(l => l.Genero)
                    .Include(l => l.Editorial)
                    .FirstOrDefault(l => l.Id == item.LibroId);

                if (libro == null)
                {
                    ModelState.AddModelError("CompraItems", $"Error! El libro con id {item.LibroId} no existe.");
                }
                else if (item.Cantidad <= 1)
                {
                    ModelState.AddModelError("CompraItems", $"Error! La cantidad de '{libro.Titulo}' debe ser mayor que 1.");
                }
                else if (libro.Stock < item.Cantidad)
                {
                    ModelState.AddModelError("CompraItems", $"Error! El libro '{libro.Titulo}' no tiene stock suficiente.");
                }
                else if (compra.CompraItems.Any(ci => ci.LibroId == libro.Id))
                {
                    ModelState.AddModelError("CompraItems", $"Error! El libro '{libro.Titulo}' está repetido.");
                }
                else
                {
                    // Todo correcto: restamos stock, añadimos el libro y sumamos al total
                    libro.Stock = libro.Stock - item.Cantidad;
                    compra.CompraItems.Add(new CompraItem(item.Cantidad, libro.Id, libro, compra.Id, compra));
                    compra.PrecioTotal = compra.PrecioTotal + libro.PrecioCompra * item.Cantidad;

                    librosComprados.Add(new CompraItemDTO(libro.Id, libro.Titulo, libro.Genero.Nombre,
                                                          libro.Editorial.Nombre, libro.PrecioCompra, item.Cantidad));
                }
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // 7. Guardamos en la base de datos
            _context.Add(compra);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Conflict("Error: " + ex.Message);
            }

            // 8. Devolvemos la compra creada
            var compraDetail = new ComprasDetailDTO(compra.Id, user.Name, user.Surname, user.Direccion,
                                                    user.PhoneNumber, compra.FechaCompra, compra.PrecioTotal,
                                                    metodoPagoTexto, compra.CodigoDescuento, librosComprados);

            return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
        }

    }
}