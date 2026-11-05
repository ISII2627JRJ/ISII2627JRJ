using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.ComprasController_test
{
    public class GetDetailsForCompras_test : AppForSEII4SqliteUT
    {
        public GetDetailsForCompras_test()
        {
            // 1. Usuario, método de pago, género, editorial y libro
            var user = new ApplicationUser("1", "Peter", "Jackson", "peter@uclm.es", "Calle Mayor 2");
            user.PhoneNumber = "678371249";
            var visa = new Visa(new DateTime(2030, 1, 1)) { NumeroTarjeta = "4123456789123456" };
            var genero = new Genero(0, "Fantasía");
            var editorial = new Editorial(0, "Minotauro");
            var libro = new Libro(0, "El Señor de los Anillos", "J.R.R. Tolkien", new DateTime(1954, 7, 29), 20.50m, 15.00m, 10, "Tapa dura", 4.9, genero, editorial);

            _context.Add(user);
            _context.Add(visa);
            _context.Add(libro);
            _context.SaveChanges();

            // 2. Una compra de 2 unidades de ese libro, pagada con Visa
            var compra = new Compra(0, new DateTime(2026, 10, 1), 41.00m, null, user.Id, user, visa.Id, visa, new List<CompraItem>());
            compra.CompraItems.Add(new CompraItem(2, libro.Id, libro, compra.Id, compra));

            _context.Add(compra);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // Act: la compra 0 no existe
            var result = await controller.GetDetailsForCompras(0);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_Found_test()
        {
            // Arrange
            var expectedCompra = new ComprasDetailDTO(1, "Peter", "Jackson", "Calle Mayor 2", "678371249",
                new DateTime(2026, 10, 1), 41.00m, "Visa", null,
                new List<CompraItemDTO>() { new CompraItemDTO(1, "El Señor de los Anillos", "Fantasía", "Minotauro", 20.50m, 2) });

            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForCompras(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var compraActual = Assert.IsType<ComprasDetailDTO>(okResult.Value);
            Assert.Equal(expectedCompra, compraActual);
        }
    }
}