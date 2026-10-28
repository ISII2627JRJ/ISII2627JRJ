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

    }
}