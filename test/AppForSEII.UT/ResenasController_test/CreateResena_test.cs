using AppForSEII.API.Controllers;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.UT
{
    public class CreateResena_test : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;

        public CreateResena_test()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task CreateResena_Error_test()
        {
            await SeedDatosAsync();

            var controller = new ResenasController(_context);

            // DTO nulo
            var dtoNulo = await controller.CreateResena(null!);
            Assert.IsType<BadRequestResult>(dtoNulo);

            // Identificador de usuario vacío
            await AssertBadRequestAsync(
                CrearSolicitud(userId: " "));

            // Título demasiado corto
            await AssertBadRequestAsync(
                CrearSolicitud(titulo: "Corto"));

            // Título demasiado largo
            await AssertBadRequestAsync(
                CrearSolicitud(titulo: "Este título es demasiado largo"));

            // Lista de libros vacía
            await AssertBadRequestAsync(
                CrearSolicitud(items: Array.Empty<ResenaItemForCreateDTO>()));

            // Lista de libros nula
            var itemsNulos = CrearSolicitud();
            itemsNulos.Items = null!;
            await AssertBadRequestAsync(itemsNulos);

            // Elemento nulo
            await AssertBadRequestAsync(
                CrearSolicitud(items: new ResenaItemForCreateDTO[] { null! }));

            // Identificador de libro no válido
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO(0, 5, null)));

            // Calificación inferior al rango permitido
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO(1, 0, null)));

            // Calificación superior al rango permitido
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO(1, 6, null)));

            // Descripción demasiado corta
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO(1, 5, "demasiado corto")));

            // Descripción demasiado larga
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO(
                        1,
                        5,
                        new string('x', 101))));

            // Usuario inexistente
            await AssertBadRequestAsync(
                CrearSolicitud(
                    userId: "usuario-inexistente",
                    items: new ResenaItemForCreateDTO(1, 5, null)));

            // Perfil incompleto: falta el nombre
            await AssertBadRequestAsync(
                CrearSolicitud(
                    userId: "sin-nombre",
                    items: new ResenaItemForCreateDTO(1, 5, null)));

            // Perfil incompleto: faltan los apellidos
            await AssertBadRequestAsync(
                CrearSolicitud(
                    userId: "sin-apellidos",
                    items: new ResenaItemForCreateDTO(1, 5, null)));

            // Perfil incompleto: falta la dirección
            await AssertBadRequestAsync(
                CrearSolicitud(
                    userId: "sin-direccion",
                    items: new ResenaItemForCreateDTO(1, 5, null)));

            // Perfil incompleto: falta el teléfono
            await AssertBadRequestAsync(
                CrearSolicitud(
                    userId: "sin-telefono",
                    items: new ResenaItemForCreateDTO(1, 5, null)));

            // No se puede repetir un libro en la misma reseña
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO[]
                    {
                        new(1, 5, null),
                        new(1, 4, null)
                    }));

            // El libro indicado no existe
            await AssertBadRequestAsync(
                CrearSolicitud(
                    items: new ResenaItemForCreateDTO(999, 5, null)));
        }

        [Fact]
        public async Task CreateResena_Success_test()
        {
            await SeedDatosAsync();

            const string comentario =
                "Una historia muy recomendable para todos.";

            var solicitud = CrearSolicitud(
                items: new ResenaItemForCreateDTO(1, 5, comentario));

            var controller = new ResenasController(_context);
            var resultado = await controller.CreateResena(solicitud);

            var created = Assert.IsType<CreatedAtActionResult>(resultado);
            Assert.Equal(
                nameof(ResenasController.GetDetailsForResenas),
                created.ActionName);

            var dto = Assert.IsType<ResenaDetailDTO>(created.Value);
            
            var dtoEsperado = new ResenaDetailDTO(
            id: dto.Id,
            tituloResena: "Reseña excelente",
            nombreCliente: "Ana",
            apellidosCliente: "Pérez",
            telefonoCliente: "600123456",
            fecha: dto.Fecha,
            items: new List<ResenaItemDTO>
            {
        new(
            libroId: 1,
            tituloLibro: "Dune",
            tipoLibro: "Tapa blanda",
            editorial: "Minotauro",
            puntuacion: 5,
            comentario: comentario)
     });

            Assert.Equal(dtoEsperado, dto);

            var resenaGuardada = await _context.Resenas
                .Include(r => r.ResenaItems)
                .SingleAsync();

            Assert.Equal("Reseña excelente", resenaGuardada.TituloResena);
            Assert.Equal("usuario-valido", resenaGuardada.ApplicationUserId);
            Assert.Single(resenaGuardada.ResenaItems);
            Assert.Equal(1, resenaGuardada.ResenaItems[0].LibroId);
            Assert.Equal(5, resenaGuardada.ResenaItems[0].Calificacion);
        }

        private async Task AssertBadRequestAsync(ResenaForCreateDTO solicitud)
        {
            var controller = new ResenasController(_context);
            var resultado = await controller.CreateResena(solicitud);

            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        }

        private static ResenaForCreateDTO CrearSolicitud(
            string userId = "usuario-valido",
            string titulo = "Reseña excelente",
            params ResenaItemForCreateDTO[] items)
        {
            return new ResenaForCreateDTO
            {
                ApplicationUserId = userId,
                TituloResena = titulo,
                Items = items.ToList()
            };
        }

        private async Task SeedDatosAsync()
        {
            var usuarioValido = CrearUsuario(
                "usuario-valido",
                "Ana",
                "Pérez",
                "Calle Mayor 1",
                "600123456");

            var usuariosConPerfilIncompleto = new[]
            {
                CrearUsuario("sin-nombre", "", "Pérez", "Calle Mayor 1", "600123456"),
                CrearUsuario("sin-apellidos", "Ana", "", "Calle Mayor 1", "600123456"),
                CrearUsuario("sin-direccion", "Ana", "Pérez", "", "600123456"),
                CrearUsuario("sin-telefono", "Ana", "Pérez", "Calle Mayor 1", "")
            };

            _context.Users.Add(usuarioValido);
            _context.Users.AddRange(usuariosConPerfilIncompleto);

            var genero = new Genero(1, "Ciencia ficción");
            var editorial = new Editorial(1, "Minotauro");

            var libro = new Libro(
                1,
                "Dune",
                "Frank Herbert",
                new DateTime(1965, 1, 1),
                20m,
                10m,
                5,
                "Tapa blanda",
                4.5,
                genero,
                editorial)
            {
                CompraItems = new List<CompraItem>(),
                ReponeItems = new List<ReposicionItem>(),
                ResenaItems = new List<ResenaItem>()
            };

            _context.Generos.Add(genero);
            _context.Editoriales.Add(editorial);
            _context.Libros.Add(libro);

            await _context.SaveChangesAsync();
        }

        private static ApplicationUser CrearUsuario(
            string id,
            string nombre,
            string apellidos,
            string direccion,
            string telefono)
        {
            return new ApplicationUser(
                id,
                nombre,
                apellidos,
                $"{id}@example.test",
                direccion)
            {
                PhoneNumber = telefono
            };
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}