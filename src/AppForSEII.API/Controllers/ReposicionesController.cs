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
            // 1. Debe haber al menos un libro
            if (dto.Libros == null || !dto.Libros.Any())
                ModelState.AddModelError("Libros", "Debe seleccionar al menos un libro para reponer.");

            // 2. El usuario debe existir
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null)
                ModelState.AddModelError("UserName", "El usuario no está registrado.");

            // 3. Comentario opcional entre 20 y 100 caracteres
            if (!string.IsNullOrWhiteSpace(dto.Comentario) &&
                (dto.Comentario.Length < 20 || dto.Comentario.Length > 100))
                ModelState.AddModelError("Comentario", "El comentario debe tener entre 20 y 100 caracteres.");

            // 4. Método de pago (Visa, GooglePay o PayPal) que ya existe en la BD
            MetodoPago? metodoPago = null;
            string metodoPagoTexto = dto.MetodoPago?.Trim().ToLower() ?? "";
            if (metodoPagoTexto == "visa")
            {
                metodoPago = await _context.MetodosPago.OfType<Visa>().FirstOrDefaultAsync();
                metodoPagoTexto = "Visa";
            }
            else if (metodoPagoTexto == "googlepay")
            {
                metodoPago = await _context.MetodosPago.OfType<GooglePay>().FirstOrDefaultAsync();
                metodoPagoTexto = "GooglePay";
            }
            else if (metodoPagoTexto == "paypal")
            {
                metodoPago = await _context.MetodosPago.OfType<PayPal>().FirstOrDefaultAsync();
                metodoPagoTexto = "PayPal";
            }

            if (metodoPago == null)
                ModelState.AddModelError("MetodoPago", "El método de pago debe ser Visa, GooglePay o PayPal.");

            if (ModelState.ErrorCount > 0)
            {
                _logger.LogError($"{DateTime.Now} Error: Datos no válidos al crear la reposición.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // 5. Guardamos en el usuario los datos del formulario
            user!.Name = dto.Nombre;
            user.Surname = dto.Apellidos;
            user.PhoneNumber = dto.Telefono;

            // 6. Creamos la reposición
            var reposicion = new Reposicion
            {
                FechaReposicion = DateTime.Now,
                Comentario = dto.Comentario,
                ApplicationUserId = user.Id,
                Usuario = user,
                MetodoPagoId = metodoPago!.Id,
                MetodoPago = metodoPago,
                ReposicionItems = new List<ReposicionItem>()
            };

            var librosRepuestos = new List<ReposicionItemDTO>();
            double precioTotal = 0;

            // 7. Recorremos cada libro del carrito
            foreach (var item in dto.Libros)
            {
                var libro = await _context.Libros.FirstOrDefaultAsync(l => l.Id == item.LibroId);

                if (libro == null)
                    ModelState.AddModelError("Libros", $"El libro con id {item.LibroId} no existe.");
                else if (item.Cantidad <= 1)
                    ModelState.AddModelError("Libros", $"La cantidad de '{libro.Titulo}' debe ser mayor que 1.");
                else if (reposicion.ReposicionItems.Any(ri => ri.LibroId == libro.Id))
                    ModelState.AddModelError("Libros", $"El libro '{libro.Titulo}' está repetido.");
                else
                {
                    reposicion.ReposicionItems.Add(new ReposicionItem
                    {
                        LibroId = libro.Id,
                        Libro = libro,
                        Cantidad = item.Cantidad,
                        Reposicion = reposicion
                    });

                    libro.Stock += item.Cantidad;
                    precioTotal += (double)libro.PrecioReposicion * item.Cantidad;

                    librosRepuestos.Add(new ReposicionItemDTO(libro.Id, libro.Titulo, libro.Autor, item.Cantidad));
                }
            }

            if (ModelState.ErrorCount > 0)
            {
                _logger.LogError($"{DateTime.Now} Error: Libros no válidos al crear la reposición.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            reposicion.PrecioTotal = precioTotal;

            // 8. Guardamos en la base de datos
            _context.Reposiciones.Add(reposicion);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{DateTime.Now} Error al guardar la reposición.");
                return Conflict("Error al guardar la reposición: " + ex.Message);
            }

            // 9. Recibo
            var resultado = new ReposicionDetailDTO(
                reposicion.Id,
                dto.Nombre,
                dto.Apellidos,
                dto.Telefono,
                reposicion.FechaReposicion,
                reposicion.PrecioTotal,
                metodoPagoTexto,
                reposicion.Comentario,
                librosRepuestos);

            return CreatedAtAction(nameof(GetDetailsForReposicion), new { id = reposicion.Id }, resultado);
        }

    }
}