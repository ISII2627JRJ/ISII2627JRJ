using AppForSEII.API.DTOs;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResenasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResenasController> _logger;

        public ResenasController(ApplicationDbContext context, ILogger<ResenasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForResenas(int id)
        {
            var resena = await _context.Resenas
                .Where(r => r.Id == id)
                .Include(r => r.ApplicationUser)                //join tabla ApplicationUser
                .Include(r => r.ResenaItems)                    //join tabla ResenaItems
                    .ThenInclude(ri => ri.Libro)                //then join tabla Libros
                        .ThenInclude(l => l.Editorial)          //then join tabla Editoriales
                .Select(r => new ResenaDetailDTO(r.Id, r.Fecha,
                    r.ApplicationUser.UserName!, r.ApplicationUser.Name!, r.ApplicationUser.Surname!,
                    r.ApplicationUser.Direccion!, r.ApplicationUser.PhoneNumber!,
                    r.TituloResena,
                    r.ResenaItems
                        .Select(ri => new ResenaItemDTO(ri.Libro.Id,
                            ri.Libro.Titulo, ri.Libro.TipoLibro, ri.Libro.Editorial.Nombre,
                            ri.Calificacion, ri.Descripcion)).ToList()))
                .FirstOrDefaultAsync();

            if (resena == null)
            {
                _logger.LogError($"Error: la reseña con id {id} no existe");
                return NotFound();
            }

            return Ok(resena);
        }


        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateResena(ResenaForCreateDTO resenaForCreate)
        {
            //las validaciones definidas en ResenaForCreateDTO y ResenaItemDTO (campos obligatorios, título 10-20,
            //calificación 1-5, descripción 20-100) se comprueban antes de ejecutar el método, así que no se repiten aquí

            if (resenaForCreate.ResenaItems == null || resenaForCreate.ResenaItems.Count == 0)
                ModelState.AddModelError("ResenaItems", "Error! Debes incluir al menos un libro para reseñar");

            //examen Sprint 2: el título de la reseña debe empezar por "Reseña:"
            if (!resenaForCreate.TituloResena.StartsWith("Reseña:"))
                ModelState.AddModelError("TituloResena", "Error! El título de la reseña debe empezar por 'Reseña:'");

            var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == resenaForCreate.UserName);
            if (user == null)
                ModelState.AddModelError("ResenaApplicationUser", "Error! El usuario no está registrado");

            if (resenaForCreate.ResenaItems != null &&
                resenaForCreate.ResenaItems.Select(ri => ri.LibroId).Distinct().Count() != resenaForCreate.ResenaItems.Count)
                ModelState.AddModelError("ResenaItems", "Error! No puedes reseñar el mismo libro más de una vez");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));


            var librosIds = resenaForCreate.ResenaItems.Select(ri => ri.LibroId).ToList();

            var libros = _context.Libros
                .Include(l => l.Editorial)
                .Where(l => librosIds.Contains(l.Id))
                .ToList();

            //se guardan en el usuario los datos introducidos en el formulario
            user!.Name = resenaForCreate.Nombre;
            user.Surname = resenaForCreate.Apellidos;
            user.Direccion = resenaForCreate.Direccion;
            user.PhoneNumber = resenaForCreate.Telefono;

            Resena resena = new Resena(0, resenaForCreate.TituloResena, DateTime.Now, user, new List<ResenaItem>());

            foreach (var item in resenaForCreate.ResenaItems)
            {
                Libro? libro = libros.FirstOrDefault(l => l.Id == item.LibroId);
                //si el libro no está en la base de datos
                if (libro == null)
                {
                    ModelState.AddModelError("ResenaItems", $"Error! El libro con id {item.LibroId} no existe");
                }
                else if (item.Calificacion < 1 || item.Calificacion > 5)
                {
                    ModelState.AddModelError("ResenaItems", $"Error! La calificación de '{libro.Titulo}' debe estar entre 1 y 5");
                }
                else
                {
                    resena.ResenaItems.Add(new ResenaItem(libro, resena, item.Calificacion, item.Descripcion));
                    //se completan los datos del libro que se devuelven en el detalle
                    item.Titulo = libro.Titulo;
                    item.TipoLibro = libro.TipoLibro;
                    item.Editorial = libro.Editorial.Nombre;
                }
            }

            //si algún libro no existe
            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _context.Add(resena);

            try
            {
                //se guardan la reseña y sus ResenaItems
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Conflict("Error! Se produjo un error al guardar la reseña, inténtalo más tarde");
            }

            //se devuelve el detalle de la reseña
            var resenaDetail = new ResenaDetailDTO(resena.Id, resena.Fecha,
                resenaForCreate.UserName, resenaForCreate.Nombre, resenaForCreate.Apellidos,
                resenaForCreate.Direccion, resenaForCreate.Telefono,
                resenaForCreate.TituloResena, resenaForCreate.ResenaItems);

            return CreatedAtAction("GetDetailsForResenas", new { id = resena.Id }, resenaDetail);
        }
    }
}