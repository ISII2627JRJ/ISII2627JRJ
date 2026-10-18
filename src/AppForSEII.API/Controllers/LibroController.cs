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
        public async Task<ActionResult> GetLibrosParaComprar()
        {
            var libros = await _context.Libros
                .Include(l => l.Genero)
                .Include(l => l.Editorial)
                .Where(l => l.Stock > 0)
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
        [Route("GetLibrosParaReponer")]
        [ProducesResponseType(typeof(IList<LibroParaReponerDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaReponer()
        {
            var libros = await _context.Libros
                .Include(l => l.Genero)
                .Where(l => l.Stock == 0)
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