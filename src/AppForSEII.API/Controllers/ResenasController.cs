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
            .ThenInclude(l => l!.Editorial)
    .FirstOrDefaultAsync(r => r.Id == id);

            if (resena == null)
            {
                return NotFound($"No se encontró la reseña con el ID {id}.");
            }

            var usuario = resena.ApplicationUser!;

            var resenaDetailDTO = new ResenaDetailDTO(
            id: resena.Id,
            tituloResena: resena.TituloResena,
            nombreCliente: usuario.Name,
            apellidosCliente: usuario.Surname,
            telefonoCliente: usuario.PhoneNumber,
            fecha: resena.Fecha,
            items: resena.ResenaItems.Select(i => new ResenaItemDTO(
            libroId: i.LibroId,
            tituloLibro: i.Libro!.Titulo,
            tipoLibro: i.Libro.TipoLibro,
            editorial: i.Libro.Editorial.Nombre,
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
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult> CreateResena(
    [FromBody] ResenaForCreateDTO resenaDTO)
        {
            if (resenaDTO == null)
            {
                return BadRequest();
            }

            var items = resenaDTO.Items
                ?? new List<ResenaItemForCreateDTO>();

            if (string.IsNullOrWhiteSpace(resenaDTO.ApplicationUserId))
            {
                ModelState.AddModelError(
                    "ApplicationUserId",
                    "El identificador del usuario es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(resenaDTO.TituloResena) ||
                resenaDTO.TituloResena.Length < 10 ||
                resenaDTO.TituloResena.Length > 20)
            {
                ModelState.AddModelError(
                    "TituloResena",
                    "El título debe tener entre 10 y 20 caracteres.");
            }

            if (items.Count == 0)
            {
                ModelState.AddModelError(
                    "Items",
                    "La reseña debe contener al menos un libro.");
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    if (item == null)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}]",
                            "El elemento de reseña no puede estar vacío.");
                        continue;
                    }

                    if (item.LibroId <= 0)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].LibroId",
                            "El identificador del libro debe ser válido.");
                    }

                    if (item.Calificacion < 1 || item.Calificacion > 5)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].Calificacion",
                            "La calificación debe estar entre 1 y 5.");
                    }

                    if (item.Descripcion != null &&
                        (item.Descripcion.Length < 20 ||
                         item.Descripcion.Length > 100))
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].Descripcion",
                            "La descripción debe tener entre 20 y 100 caracteres.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var usuario = await _context.Users.FindAsync(
                resenaDTO.ApplicationUserId);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "ApplicationUserId",
                    "El usuario especificado no existe en el sistema.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (string.IsNullOrWhiteSpace(usuario.Name) ||
                string.IsNullOrWhiteSpace(usuario.Surname) ||
                string.IsNullOrWhiteSpace(usuario.Direccion) ||
                string.IsNullOrWhiteSpace(usuario.PhoneNumber))
            {
                ModelState.AddModelError(
                    "ApplicationUserId",
                    "El perfil debe tener nombre, apellidos, dirección y teléfono.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var idsSolicitados = items
                .Select(item => item.LibroId)
                .ToList();

            var idsUnicos = idsSolicitados
                .Distinct()
                .ToList();

            if (idsSolicitados.Count != idsUnicos.Count)
            {
                ModelState.AddModelError(
                    "Items",
                    "No se puede repetir un libro en la reseña.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var librosPorId = await _context.Libros
                .Include(libro => libro.Editorial)
                .Where(libro => idsUnicos.Contains(libro.Id))
                .ToDictionaryAsync(libro => libro.Id);

            if (librosPorId.Count != idsUnicos.Count)
            {
                ModelState.AddModelError(
                    "Items",
                    "Uno o más libros no existen.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var resena = new Resena(
     id: 0,
     fecha: DateTime.Now,
     applicationUserId: resenaDTO.ApplicationUserId)
            {
                TituloResena = resenaDTO.TituloResena
            };

            foreach (var itemDTO in items)
            {
                resena.ResenaItems.Add(
                    new ResenaItem(
                        libroId: itemDTO.LibroId,
                        resenaId: resena.Id,
                        descripcion: itemDTO.Descripcion,
                        calificacion: itemDTO.Calificacion)
                    {
                        Resena = resena
                    });
            }


            try
            {
                _context.Resenas.Add(resena);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Problem(
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "No se pudo guardar la reseña.",
                    detail: "Se produjo un error al guardar la reseña en la base de datos.");
            }

            var detalle = new ResenaDetailDTO(
                id: resena.Id,
                tituloResena: resena.TituloResena,
                nombreCliente: usuario.Name,
                apellidosCliente: usuario.Surname,
                telefonoCliente: usuario.PhoneNumber,
                fecha: resena.Fecha,
                items: resena.ResenaItems.Select(item =>
                {
                    var libro = librosPorId[item.LibroId];

                    return new ResenaItemDTO(
                        libroId: item.LibroId,
                        tituloLibro: libro.Titulo,
                        tipoLibro: libro.TipoLibro,
                        editorial: libro.Editorial.Nombre,
                        puntuacion: item.Calificacion,
                        comentario: item.Descripcion);
                }).ToList()
            );

            return CreatedAtAction(
                nameof(GetDetailsForResenas),
                new { id = resena.Id },
                detalle);
        }
    }
}