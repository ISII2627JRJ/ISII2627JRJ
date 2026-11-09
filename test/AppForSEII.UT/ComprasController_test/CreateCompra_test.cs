using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.ComprasController_test
{
    public class CreateCompra_test : AppForSEII4SqliteUT
    {
        public CreateCompra_test()
        {
            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es", "Calle Mayor 2");
            user.PhoneNumber = "678371249";

            var visa = new Visa(new DateTime(2030, 1, 1)) { NumeroTarjeta = "4123456789123456" };
            var googlePay = new GooglePay("pagos@uclm.es");
            var payPal = new PayPal("600123456");

            var genero = new Genero(0, "Fantasía");
            var editorial = new Editorial(0, "Minotauro");
            // Libro 1 con stock 10
            var libro = new Libro(0, "El Señor de los Anillos", "J.R.R. Tolkien", new DateTime(1954, 7, 29), 20.50m, 15.00m, 10, "Tapa dura", 4.9, genero, editorial);

            _context.Add(user);
            _context.AddRange(visa, googlePay, payPal);
            _context.Add(libro);
            _context.SaveChanges();
        }

        // Casos de error: (compra que enviamos, mensaje de error esperado)
        public static IEnumerable<object[]> TestCasesFor_CreateCompra()
        {
            var sinLibros = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>());

            var librosNulos = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>());
            librosNulos.CompraItems = null!;

            var usuarioNoExiste = new CompraForCreateDTO("noexiste@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 2) });

            var metodoPagoMal = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 7, null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 2) });

            var libroNoExiste = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>() { new CompraItemDTO(99, "", "", "", 0, 2) });

            var cantidadUno = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 1) });

            var sinStock = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 500) });

            var libroRepetido = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 2), new CompraItemDTO(1, "", "", "", 0, 2) });

          var direccionMal = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "C/Mayor 2", "678371249", 1, null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 2) });
            var allTests = new List<object[]>
            {
                new object[] { sinLibros, "Error! Debes incluir al menos un libro para comprar." },
                new object[] { librosNulos, "Error! Debes incluir al menos un libro para comprar." },
                new object[] { usuarioNoExiste, "Error! El usuario no está registrado." },
                new object[] { metodoPagoMal, "Error! Método de pago no válido." },
                new object[] { libroNoExiste, "Error! El libro con id 99 no existe." },
                new object[] { cantidadUno, "Error! La cantidad de 'El Señor de los Anillos' debe ser mayor que 1." },
                new object[] { sinStock, "Error! El libro 'El Señor de los Anillos' no tiene stock suficiente." },
                new object[] { libroRepetido, "Error! El libro 'El Señor de los Anillos' está repetido." },
                new object[] { direccionMal, "Error! La direccion debe incluir la palabra calle." }

            };
            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_CreateCompra))]
        public async Task CreateCompra_Error_test(CompraForCreateDTO compraForCreate, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.CreateCompra(compraForCreate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            Assert.Equal(errorExpected, problemDetails.Errors.First().Value[0]);
        }

        // Compra correcta con cada uno de los 3 métodos de pago
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [InlineData(1, "Visa")]
        [InlineData(2, "GooglePay")]
        [InlineData(3, "PayPal")]
        public async Task CreateCompra_Success_test(int metodoPago, string metodoPagoEsperado)
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            var compraForCreate = new CompraForCreateDTO("peter@uclm.es", "Peter", "Jackson", "Calle Mayor 2", "678371249", metodoPago, "DTO10",
                new List<CompraItemDTO>() { new CompraItemDTO(1, "", "", "", 0, 2) });

            var expectedCompra = new ComprasDetailDTO(1, "Peter", "Jackson", "Calle Mayor 2", "678371249",
                DateTime.Now, 41.00m, metodoPagoEsperado, "DTO10",
                new List<CompraItemDTO>() { new CompraItemDTO(1, "El Señor de los Anillos", "Fantasía", "Minotauro", 20.50m, 2) });

            // Act
            var result = await controller.CreateCompra(compraForCreate);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var compraActual = Assert.IsType<ComprasDetailDTO>(createdResult.Value);

            // La fecha la pone el controlador con DateTime.Now, así que copiamos la real para comparar el resto
            expectedCompra.FechaCompra = compraActual.FechaCompra;
            Assert.Equal(expectedCompra, compraActual);

            // Comprobamos que se ha restado el stock: 10 - 2 = 8
            Assert.Equal(8, _context.Libros.First(l => l.Id == 1).Stock);
        }
    }
}