using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;

namespace AppForSEII.UT.LibroController_test
{
    public class GetLibrosParaResenar_test : AppForSEII4SqliteUT
    {
        // Constructor: mete en la BD de pruebas los datos que necesitamos
        public GetLibrosParaResenar_test()
        {
            var generos = new List<Genero>()
            {
                new Genero(0, "Ciencia ficción"),
                new Genero(0, "Fantasía")
            };
            var editoriales = new List<Editorial>()
            {
                new Editorial(0, "Minotauro"),
                new Editorial(0, "Nova")
            };
            var libros = new List<Libro>()
            {
                new Libro(0, "Dune", "Frank Herbert", new DateTime(1965, 8, 1), 20m, 10m, 5, "Tapa blanda", 4.5, generos[0], editoriales[0]),
                new Libro(0, "Fundación", "Isaac Asimov", new DateTime(1951, 1, 1), 18m, 9m, 2, "Tapa dura premium", 4.6, generos[0], editoriales[1]),
                // Este libro tiene stock 0, pero para reseñar se muestran todos los libros del sistema
                new Libro(0, "El Hobbit", "J. R. R. Tolkien", new DateTime(1937, 9, 21), 15m, 7m, 0, "Tapa blanda", 4.7, generos[1], editoriales[0])
            };

            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // Casos de prueba: (genero, tipoLibro, libros que esperamos)
        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaResenar_OK()
        {
            var dune = new LibroParaResenarDTO(1, "Dune", "Tapa blanda", "Ciencia ficción", "Minotauro", "Frank Herbert", 4.5, new DateTime(1965, 8, 1));
            var fundacion = new LibroParaResenarDTO(2, "Fundación", "Tapa dura premium", "Ciencia ficción", "Nova", "Isaac Asimov", 4.6, new DateTime(1951, 1, 1));
            var hobbit = new LibroParaResenarDTO(3, "El Hobbit", "Tapa blanda", "Fantasía", "Minotauro", "J. R. R. Tolkien", 4.7, new DateTime(1937, 9, 21));

            var allTests = new List<object?[]>
            {
                // Sin filtros: todos los libros, ordenados por título
                new object?[] { null, null, new List<LibroParaResenarDTO>() { dune, hobbit, fundacion } },
                // Filtro por género
                new object?[] { "Ciencia ficción", null, new List<LibroParaResenarDTO>() { dune, fundacion } },
                // Filtro por tipo de libro
                new object?[] { null, "Tapa blanda", new List<LibroParaResenarDTO>() { dune, hobbit } },
                // Filtro por género y tipo de libro a la vez
                new object?[] { "Ciencia ficción", "Tapa blanda", new List<LibroParaResenarDTO>() { dune } },
                // Combinación que no cumple ningún libro: no debe devolver nada
                new object?[] { "Fantasía", "Tapa dura premium", new List<LibroParaResenarDTO>() }
            };
            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_GetLibrosParaResenar_OK))]
        public async Task GetLibrosParaResenar_OK_test(string? genero, string? tipoLibro, List<LibroParaResenarDTO> expectedLibros)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaResenar(genero, tipoLibro);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var librosActual = Assert.IsType<List<LibroParaResenarDTO>>(okResult.Value);
            Assert.Equal(expectedLibros, librosActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaResenar_badrequest_genero_test()
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act: filtro de género vacío -> error
            var result = await controller.GetLibrosParaResenar(" ", null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Equal("El parámetro de género no puede estar vacío.", problemDetails.Errors.First().Value[0]);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaResenar_badrequest_tipoLibro_test()
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act: filtro de tipo de libro vacío -> error
            var result = await controller.GetLibrosParaResenar(null, " ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Equal("El parámetro de tipo de libro no puede estar vacío.", problemDetails.Errors.First().Value[0]);
        }
    }
}