using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;

namespace AppForSEII.UT.ResenasController_test
{
    public class GetResenas_test : AppForSEII4SqliteUT
    {
               public GetResenas_test()
        {
            var generos = new List<Genero>() {
                new Genero(0, "Ciencia ficción"),
            };

            var editoriales = new List<Editorial>() {
                new Editorial(0, "Minotauro"),
            };

            var libros = new List<Libro>() {
                new Libro(0, "Dune", "Frank Herbert", new DateTime(1965, 1, 1), 20m, 10m, 5, "Tapa blanda", 4.5, generos[0], editoriales[0]),
            };

            ApplicationUser user = new ApplicationUser("1", "Ana", "Pérez", "ana@uclm.es", "Calle Mayor 1");
            user.PhoneNumber = "600123456";

            var resena = new Resena(0, "Reseña: buena obra", new DateTime(2026, 11, 1), user, new List<ResenaItem>());
            resena.ResenaItems.Add(new ResenaItem(libros[0], resena, 5, "Una novela de ciencia ficción muy recomendable."));

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.Add(resena);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetResena_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ResenasController>>();
            ILogger<ResenasController> logger = mock.Object;

            var controller = new ResenasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForResenas(0);

            //Assert
            //comprobamos que la respuesta es NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetResena_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ResenasController>>();
            ILogger<ResenasController> logger = mock.Object;

            var controller = new ResenasController(_context, logger);

            var expectedResena = new ResenaDetailDTO(1, new DateTime(2026, 11, 1),
                "ana@uclm.es", "Ana", "Pérez", "Calle Mayor 1", "600123456",
                "Reseña: buena obra", new List<ResenaItemDTO>());
            expectedResena.ResenaItems.Add(new ResenaItemDTO(1, "Dune", "Tapa blanda", "Minotauro", 5,
                "Una novela de ciencia ficción muy recomendable."));

            // Act
            var result = await controller.GetDetailsForResenas(1);

            //Assert
            //comprobamos que la respuesta es OK y obtenemos la reseña
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resenaDTOActual = Assert.IsType<ResenaDetailDTO>(okResult.Value);

            //comprobamos que la reseña esperada y la obtenida son iguales
            Assert.Equal(expectedResena, resenaDTOActual);
        }
    }
}