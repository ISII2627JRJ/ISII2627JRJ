using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.LibroController_test
{
    public class GetLibrosParaReponer_test : AppForSEII4SqliteUT
    {
        // Constructor: mete en la BD de pruebas los datos que necesitamos
        public GetLibrosParaReponer_test()
        {
            var generos = new List<Genero>()
            {
                new Genero(0, "Terror"),
                new Genero(0, "Romántica"),
                new Genero(0, "Fantasía")
            };
            var editoriales = new List<Editorial>()
            {
                new Editorial(0, "Plaza & Janés"),
                new Editorial(0, "Salamandra"),
                new Editorial(0, "Minotauro")
            };
            var libros = new List<Libro>()
            {
                // Libros sin stock (stock = 0): son los que se pueden reponer
                new Libro(0, "El Resplandor", "Stephen King", new DateTime(1977, 1, 28), 22.00m, 16.00m, 0, "Edición tapa dura", 4.5, generos[0], editoriales[0]),
                new Libro(0, "Orgullo y prejuicio", "Jane Austen", new DateTime(1813, 1, 28), 9.95m, 6.00m, 0, "Edición de bolsillo", 4.3, generos[1], editoriales[1]),
                new Libro(0, "It", "Stephen King", new DateTime(1986, 9, 15), 25.00m, 18.00m, 0, "Edición de bolsillo", 4.6, generos[0], editoriales[0]),
                // Este libro tiene stock, así que NO debe salir nunca
                new Libro(0, "El Señor de los Anillos", "J.R.R. Tolkien", new DateTime(1954, 7, 29), 20.50m, 15.00m, 10, "Edición tapa dura", 4.9, generos[2], editoriales[2])
            };

            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // Casos de prueba: (autor, genero, libros que esperamos)
        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaReponer_OK()
        {
            var resplandor = new LibroParaReponerDTO(1, "El Resplandor", "Terror", new DateTime(1977, 1, 28), 16.00m, "Stephen King");
            var orgullo = new LibroParaReponerDTO(2, "Orgullo y prejuicio", "Romántica", new DateTime(1813, 1, 28), 6.00m, "Jane Austen");
            var it = new LibroParaReponerDTO(3, "It", "Terror", new DateTime(1986, 9, 15), 18.00m, "Stephen King");

            var allTests = new List<object[]>
            {
                // Sin filtros: todos los que tienen stock 0, ordenados por título
                new object[] { null, null, new List<LibroParaReponerDTO>() { resplandor, it, orgullo } },
                // Filtro por autor
                new object[] { "Stephen King", null, new List<LibroParaReponerDTO>() { resplandor, it } },
                // Filtro por género
                new object[] { null, "Romántica", new List<LibroParaReponerDTO>() { orgullo } },
                // Filtro por autor y género a la vez
                new object[] { "Jane Austen", "Romántica", new List<LibroParaReponerDTO>() { orgullo } },
                // Autor de un libro con stock: no debe devolver nada
                new object[] { "J.R.R. Tolkien", null, new List<LibroParaReponerDTO>() }
            };
            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_GetLibrosParaReponer_OK))]
        public async Task GetLibrosParaReponer_OK_test(string? autor, string? genero, List<LibroParaReponerDTO> expectedLibros)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaReponer(autor, genero);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var librosActual = Assert.IsType<List<LibroParaReponerDTO>>(okResult.Value);
            Assert.Equal(expectedLibros, librosActual);
        }

        // Casos de error: (autor, genero, mensaje de error esperado)
        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaReponer_badrequest()
        {
            var allTests = new List<object[]>
            {
                // Filtro de autor vacío
                new object[] { " ", null, "El filtro de autor no puede estar vacío si se proporciona." },
                // Filtro de género vacío
                new object[] { null, " ", "El filtro de género no puede estar vacío si se proporciona." }
            };
            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_GetLibrosParaReponer_badrequest))]
        public async Task GetLibrosParaReponer_badrequest_test(string? autor, string? genero, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaReponer(autor, genero);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Equal(errorExpected, problemDetails.Errors.First().Value[0]);
        }
    }
}