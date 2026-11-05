using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.LibroController_test
{
    public class GetLibrosParaComprar_test : AppForSEII4SqliteUT
    {
        // Constructor: mete en la BD de pruebas los datos que necesitamos
        public GetLibrosParaComprar_test()
        {
            var generos = new List<Genero>()
            {
                new Genero(0, "Fantasía"),
                new Genero(0, "Ciencia Ficción"),
                new Genero(0, "Terror")
            };
            var editoriales = new List<Editorial>()
            {
                new Editorial(0, "Minotauro"),
                new Editorial(0, "Gigamesh"),
                new Editorial(0, "Plaza & Janés")
            };
            var libros = new List<Libro>()
            {
                new Libro(0, "El Señor de los Anillos", "J.R.R. Tolkien", new DateTime(1954, 7, 29), 20.50m, 15.00m, 10, "Tapa dura", 4.9, generos[0], editoriales[0]),
                new Libro(0, "Dune", "Frank Herbert", new DateTime(1965, 8, 1), 18.90m, 12.50m, 15, "Bolsillo", 4.8, generos[1], editoriales[1]),
                // Este libro tiene stock 0, así que NO debe salir nunca
                new Libro(0, "El Resplandor", "Stephen King", new DateTime(1977, 1, 28), 22.00m, 16.00m, 0, "Tapa dura", 4.5, generos[2], editoriales[2])
            };

            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // Casos de prueba: (genero, precioMax, libros que esperamos)
        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
            var dune = new LibroParaComprarDTO(2, "Dune", 18.90m, new DateTime(1965, 8, 1), "Ciencia Ficción", "Gigamesh");
            var anillos = new LibroParaComprarDTO(1, "El Señor de los Anillos", 20.50m, new DateTime(1954, 7, 29), "Fantasía", "Minotauro");

            var allTests = new List<object[]>
            {
                // Sin filtros: todos los que tienen stock, ordenados por título
                new object[] { null, null, new List<LibroParaComprarDTO>() { dune, anillos } },
                // Filtro por género
                new object[] { "Fantasía", null, new List<LibroParaComprarDTO>() { anillos } },
                // Filtro por precio máximo
                new object[] { null, 19m, new List<LibroParaComprarDTO>() { dune } }
            };
            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]
        public async Task GetLibrosParaComprar_OK_test(string? genero, decimal? precioMax, List<LibroParaComprarDTO> expectedLibros)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaComprar(genero, precioMax);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var librosActual = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);
            Assert.Equal(expectedLibros, librosActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_badrequest_test()
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act: precio negativo -> error
            var result = await controller.GetLibrosParaComprar(null, -1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Equal("El precio máximo filtrado no puede ser un valor negativo.", problemDetails.Errors.First().Value[0]);
        }
    }
}