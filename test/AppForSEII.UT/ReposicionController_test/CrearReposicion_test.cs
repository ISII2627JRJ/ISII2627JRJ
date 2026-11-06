using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.ReposicionesController_test
{
    public class CrearReposicion_test : AppForSEII4SqliteUT
    {
        public CrearReposicion_test()
        {
            var admin = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es", "Calle Principal 1");
            admin.PhoneNumber = "678371248";

            var visa = new Visa(new DateTime(2030, 1, 1)) { NumeroTarjeta = "4123456789123456" };
            var googlePay = new GooglePay("pagos@uclm.es");
            var payPal = new PayPal("600123456");

            var genero = new Genero(0, "Terror");
            var editorial = new Editorial(0, "Plaza & Janés");
            // Libro 1 y libro 2 sin stock (son los que se reponen)
            var resplandor = new Libro(0, "El Resplandor", "Stephen King", new DateTime(1977, 1, 28), 22.00m, 16.00m, 0, "Edición tapa dura", 4.5, genero, editorial);
            var it = new Libro(0, "It", "Stephen King", new DateTime(1986, 9, 15), 25.00m, 18.00m, 0, "Edición de bolsillo", 4.6, genero, editorial);

            _context.Add(admin);
            _context.AddRange(visa, googlePay, payPal);
            _context.AddRange(resplandor, it);
            _context.SaveChanges();
        }

        // Método auxiliar para crear la petición (el DTO no tiene constructor)
        private static ReposicionForCreateDTO CrearDTO(string userName, string metodoPago, string? comentario,
            List<ReposicionForCreateDTO.ItemDTO> libros)
        {
            return new ReposicionForCreateDTO(userName, "Elena", "Navarro Martínez", "678371248", metodoPago, comentario, libros);
        }

        private static ReposicionForCreateDTO.ItemDTO Item(int libroId, int cantidad)
        {
            return new ReposicionForCreateDTO.ItemDTO(libroId, cantidad);
        }
        // Casos de error: (reposición que enviamos, mensaje de error esperado)
        public static IEnumerable<object[]> TestCasesFor_CrearReposicion()
        {
            var sinLibros = CrearDTO("elena@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>());

            var librosNulos = CrearDTO("elena@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>());
            librosNulos.Libros = null!;

            var usuarioNoExiste = CrearDTO("noexiste@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2) });

            var comentarioCorto = CrearDTO("elena@uclm.es", "Visa", "Muy corto",
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2) });

            var comentarioLargo = CrearDTO("elena@uclm.es", "Visa", new string('a', 101),
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2) });

            var metodoPagoMal = CrearDTO("elena@uclm.es", "Bizum", null,
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2) });

            var libroNoExiste = CrearDTO("elena@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(99, 2) });

            var cantidadUno = CrearDTO("elena@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 1) });

            var libroRepetido = CrearDTO("elena@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2), Item(1, 3) });
            var metodoPagoNulo = CrearDTO("elena@uclm.es", "Visa", null,
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2) });
            metodoPagoNulo.MetodoPago = null!;

            var allTests = new List<object[]>
            {
                new object[] { sinLibros, "Debe seleccionar al menos un libro para reponer." },
                new object[] { librosNulos, "Debe seleccionar al menos un libro para reponer." },
                new object[] { usuarioNoExiste, "El usuario no está registrado." },
                new object[] { comentarioCorto, "El comentario debe tener entre 20 y 100 caracteres." },
                new object[] { comentarioLargo, "El comentario debe tener entre 20 y 100 caracteres." },
                new object[] { metodoPagoMal, "El método de pago debe ser Visa, GooglePay o PayPal." },
                new object[] { libroNoExiste, "El libro con id 99 no existe." },
                new object[] { cantidadUno, "La cantidad de 'El Resplandor' debe ser mayor que 1." },
                new object[] { libroRepetido, "El libro 'El Resplandor' está repetido." },
                new object[] { metodoPagoNulo, "El método de pago debe ser Visa, GooglePay o PayPal." }            };
            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_CrearReposicion))]
        public async Task CrearReposicion_Error_test(ReposicionForCreateDTO reposicionForCreate, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // Act
            var result = await controller.CrearReposicion(reposicionForCreate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Equal(errorExpected, problemDetails.Errors.First().Value[0]);

            // No se ha guardado ninguna reposición
            Assert.Empty(_context.Reposiciones);
        }

        // Reposición correcta con cada uno de los 3 métodos de pago
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [InlineData("Visa", "Visa")]
        [InlineData("googlepay", "GooglePay")]
        [InlineData("PAYPAL", "PayPal")]
        public async Task CrearReposicion_Success_test(string metodoPago, string metodoPagoEsperado)
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // 2 "El Resplandor" (2*16) + 3 "It" (3*18) = 86
            var reposicionForCreate = CrearDTO("elena@uclm.es", metodoPago, "Reposición urgente de libros sin stock",
                new List<ReposicionForCreateDTO.ItemDTO>() { Item(1, 2), Item(2, 3) });

            var expectedReposicion = new ReposicionDetailDTO(1, "Elena", "Navarro Martínez", "678371248",
                DateTime.Now, 86.00, metodoPagoEsperado, "Reposición urgente de libros sin stock",
                new List<ReposicionItemDTO>()
                {
                    new ReposicionItemDTO(1, "El Resplandor", "Stephen King", 2),
                    new ReposicionItemDTO(2, "It", "Stephen King", 3)
                });

            // Act
            var result = await controller.CrearReposicion(reposicionForCreate);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var reposicionActual = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

            // La fecha la pone el controlador con DateTime.Now, así que copiamos la real para comparar el resto
            expectedReposicion.FechaReposicion = reposicionActual.FechaReposicion;
            Assert.Equal(expectedReposicion, reposicionActual);

            // Se ha guardado la reposición y se ha sumado el stock: 0 + 2 y 0 + 3
            Assert.Single(_context.Reposiciones);
            Assert.Equal(2, _context.Libros.First(l => l.Id == 1).Stock);
            Assert.Equal(3, _context.Libros.First(l => l.Id == 2).Stock);
        }
    }
}