using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;

namespace AppForSEII.UT.ResenasController_test
{
    public class CreateResena_test : AppForSEII4SqliteUT
    {
        private const string _userName = "ana@uclm.es";
        private const string _nombre = "Ana";
        private const string _apellidos = "Pérez López";
        private const string _direccion = "Calle Mayor 1, Albacete";
        private const string _telefono = "600123456";
        private const string _tituloResena = "Reseña: me gusta";

        private const string _libro1Titulo = "Dune";
        private const string _libro2Titulo = "El Hobbit";
        private const string _tipoLibro = "Tapa blanda";
        private const string _editorial = "Minotauro";

                public CreateResena_test()
        {
            var generos = new List<Genero>() {
                new Genero(0, "Ciencia ficción"),
                new Genero(0, "Fantasía"),
            };

            var editoriales = new List<Editorial>() {
                new Editorial(0, _editorial),
            };

            var libros = new List<Libro>() {
                new Libro(0, _libro1Titulo, "Frank Herbert", new DateTime(1965, 1, 1), 20m, 10m, 5, _tipoLibro, 4.5, generos[0], editoriales[0]),
                new Libro(0, _libro2Titulo, "J. R. R. Tolkien", new DateTime(1937, 9, 21), 15m, 7m, 3, _tipoLibro, 4.7, generos[1], editoriales[0]),
            };

            ApplicationUser user = new ApplicationUser("1", "Ana", "Pérez", _userName, "Calle Antigua 5");
            user.PhoneNumber = "600000000";

            //ya existe una reseña, así que la nueva tendrá Id=2
            var resena = new Resena(0, "Reseña: buena obra", DateTime.Now, user, new List<ResenaItem>());
            resena.ResenaItems.Add(new ResenaItem(libros[0], resena, 5, "Una novela de ciencia ficción muy recomendable."));

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.Add(resena);
            _context.SaveChanges();
        }

                public static IEnumerable<object[]> TestCasesFor_CreateResena()
        {
            var resenaItems = new List<ResenaItemDTO>() { new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 4) };

            var resenaNoItem = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>());

            var resenaItemsNulos = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>());
            resenaItemsNulos.ResenaItems = null!;

            var resenaTituloSinPrefijo = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                "El libro me gusta", resenaItems);

            var resenaApplicationUser = new ResenaForCreateDTO("noexiste@uclm.es", _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, resenaItems);

            var resenaLibroRepetido = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>() {
                    new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 4),
                    new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 5) });

            var resenaLibroNoExiste = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>() { new ResenaItemDTO(99, "No existe", _tipoLibro, _editorial, 4) });

            var resenaCalificacionCero = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>() { new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 0) });

            var resenaCalificacionSeis = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>() { new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 6) });

            var allTests = new List<object[]>
            {             //entrada para CreateResena - error esperado
                new object[] { resenaNoItem, "Error! Debes incluir al menos un libro para reseñar" },
                new object[] { resenaItemsNulos, "Error! Debes incluir al menos un libro para reseñar" },
                new object[] { resenaTituloSinPrefijo, "Error! El título de la reseña debe empezar por 'Reseña:'" },
                new object[] { resenaApplicationUser, "Error! El usuario no está registrado" },
                new object[] { resenaLibroRepetido, "Error! No puedes reseñar el mismo libro más de una vez" },
                new object[] { resenaLibroNoExiste, "Error! El libro con id 99 no existe" },
                new object[] { resenaCalificacionCero, "Error! La calificación de 'El Hobbit' debe estar entre 1 y 5" },
                new object[] { resenaCalificacionSeis, "Error! La calificación de 'El Hobbit' debe estar entre 1 y 5" },
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_CreateResena))]
        public async Task CreateResena_Error_test(ResenaForCreateDTO resenaDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ResenasController>>();
            ILogger<ResenasController> logger = mock.Object;

            var controller = new ResenasController(_context, logger);

            // Act
            var result = await controller.CreateResena(resenaDTO);

            //Assert
            //comprobamos que la respuesta es BadRequest y obtenemos el error devuelto
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            //comprobamos que el error esperado y el obtenido son iguales
            Assert.Equal(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateResena_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ResenasController>>();
            ILogger<ResenasController> logger = mock.Object;

            var controller = new ResenasController(_context, logger);

            var resenaDTO = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>()
                { new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 4, "Una aventura preciosa para leer en familia.") });

            var expectedResenaDetailDTO = new ResenaDetailDTO(2, DateTime.Now,
                _userName, _nombre, _apellidos, _direccion, _telefono,
                _tituloResena, new List<ResenaItemDTO>()
                { new ResenaItemDTO(2, _libro2Titulo, _tipoLibro, _editorial, 4, "Una aventura preciosa para leer en familia.") });

            // Act
            var result = await controller.CreateResena(resenaDTO);

            //Assert
            //comprobamos que la respuesta es Created y obtenemos la reseña creada
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualResenaDetailDTO = Assert.IsType<ResenaDetailDTO>(createdResult.Value);

            //comprobamos que la reseña esperada y la obtenida son iguales
            Assert.Equal(expectedResenaDetailDTO, actualResenaDetailDTO);
            
            // Comprobamos que la reseña se ha guardado (ya había 1) y que se han actualizado los datos del usuario
            Assert.Equal(2, _context.Resenas.Count());
            Assert.Equal(_telefono, _context.ApplicationUsers.First(u => u.UserName == _userName).PhoneNumber);
        }
    }
}