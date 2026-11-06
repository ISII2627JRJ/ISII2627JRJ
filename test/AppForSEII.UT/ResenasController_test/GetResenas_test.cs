using AppForSEII.API.Controllers;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.UT
{
    public class GetResenas_test : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;

        public GetResenas_test()
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
        public async Task GetResena_NotFound_test()
        {
            var controller = new ResenasController(_context);

            var result = await controller.GetDetailsForResenas(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetResena_Found_test()
        {
            var cliente = new ApplicationUser(
                "cliente-test",
                "Ana",
                "Pérez",
                "ana@example.test",
                "Calle Mayor 1")
            {
                PhoneNumber = "600123456"
            };

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

            const string descripcion =
                "Una novela de ciencia ficción muy recomendable.";

            var resena = new Resena(
                1,
                new DateTime(2026, 11, 1),
                cliente.Id)
            {
                TituloResena = "Una buena obra",
                ApplicationUser = cliente
            };

            var item = new ResenaItem(
                libro.Id,
                resena.Id,
                descripcion,
                5)
            {
                Libro = libro,
                Resena = resena
            };

            resena.ResenaItems.Add(item);
            libro.ResenaItems.Add(item);

            _context.Generos.Add(genero);
            _context.Editoriales.Add(editorial);
            _context.Libros.Add(libro);
            _context.Users.Add(cliente);
            _context.Resenas.Add(resena);

            await _context.SaveChangesAsync();

            var controller = new ResenasController(_context);
            var result = await controller.GetDetailsForResenas(resena.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ResenaDetailDTO>(okResult.Value);

            var dtoEsperado = new ResenaDetailDTO(
             id: resena.Id,
            tituloResena: "Una buena obra",
            nombreCliente: "Ana",
            apellidosCliente: "Pérez",
            telefonoCliente: "600123456",
            fecha: new DateTime(2026, 11, 1),
            items: new List<ResenaItemDTO>
            {
        new(
            libroId: libro.Id,
            tituloLibro: "Dune",
            tipoLibro: "Tapa blanda",
            editorial: "Minotauro",
            puntuacion: 5,
            comentario: descripcion)
     });

            Assert.Equal(dtoEsperado, dto);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}