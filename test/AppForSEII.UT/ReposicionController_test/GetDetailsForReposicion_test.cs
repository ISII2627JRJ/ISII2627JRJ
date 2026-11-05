using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT.ReposicionesController_test
{
    public class GetDetailsForReposicion_test : AppForSEII4SqliteUT
    {
        public GetDetailsForReposicion_test()
        {
            // 1. Usuario (admin), método de pago, género, editorial y libros sin stock
            var admin = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es", "Calle Principal 1");
            admin.PhoneNumber = "678371248";
            var visa = new Visa(new DateTime(2030, 1, 1)) { NumeroTarjeta = "4123456789123456" };
            var genero = new Genero(0, "Terror");
            var editorial = new Editorial(0, "Plaza & Janés");
            var resplandor = new Libro(0, "El Resplandor", "Stephen King", new DateTime(1977, 1, 28), 22.00m, 16.00m, 0, "Edición tapa dura", 4.5, genero, editorial);
            var it = new Libro(0, "It", "Stephen King", new DateTime(1986, 9, 15), 25.00m, 18.00m, 0, "Edición de bolsillo", 4.6, genero, editorial);

            _context.Add(admin);
            _context.Add(visa);
            _context.AddRange(resplandor, it);
            _context.SaveChanges();

            // 2. Una reposición de 2 "El Resplandor" (2*16) y 3 "It" (3*18) = 86, pagada con Visa
            var reposicion = new Reposicion(0, new DateTime(2026, 10, 1), 86.00, "Reposición urgente de libros sin stock");
            reposicion.ApplicationUserId = admin.Id;
            reposicion.Usuario = admin;
            reposicion.MetodoPagoId = visa.Id;
            reposicion.MetodoPago = visa;
            reposicion.ReposicionItems.Add(new ReposicionItem { Cantidad = 2, LibroId = resplandor.Id, Libro = resplandor, Reposicion = reposicion });
            reposicion.ReposicionItems.Add(new ReposicionItem { Cantidad = 3, LibroId = it.Id, Libro = it, Reposicion = reposicion });

            _context.Add(reposicion);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForReposicion_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // Act: la reposición 0 no existe
            var result = await controller.GetDetailsForReposicion(0);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForReposicion_Found_test()
        {
            // Arrange
            var expectedReposicion = new ReposicionDetailDTO(1, "Elena", "Navarro Martínez", "678371248",
                new DateTime(2026, 10, 1), 86.00, "Visa", "Reposición urgente de libros sin stock",
                new List<ReposicionItemDTO>()
                {
                    new ReposicionItemDTO(1, "El Resplandor", "Stephen King", 2),
                    new ReposicionItemDTO(2, "It", "Stephen King", 3)
                });

            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // Act
            var result = await controller.GetDetailsForReposicion(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reposicionActual = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            Assert.Equal(expectedReposicion, reposicionActual);
        }
    }
}