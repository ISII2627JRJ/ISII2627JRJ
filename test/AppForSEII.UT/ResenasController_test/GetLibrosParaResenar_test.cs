using AppForSEII.API.Controllers;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppForSEII.UT
{
    public class GetLibrosParaResenar_test : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;

        public GetLibrosParaResenar_test()
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
        public async Task GetLibrosParaResenar_OK_test()
        {
            await SeedLibrosAsync();

            var controller = new LibroController(
                _context,
                Mock.Of<ILogger<LibroController>>());

            var resultadoSinFiltros =
                await controller.GetLibrosParaResenar(null, null);

            var okSinFiltros =
                Assert.IsType<OkObjectResult>(resultadoSinFiltros);

            var todosLosLibros =
                Assert.IsType<List<LibroParaResenarDTO>>(okSinFiltros.Value);

            Assert.Equal(3, todosLosLibros.Count);
            Assert.Contains(todosLosLibros, libro => libro.Id == 3);

            var resultadoFiltrado =
                await controller.GetLibrosParaResenar(
                    "Ciencia ficción",
                    "Tapa blanda");

            var okFiltrado =
                Assert.IsType<OkObjectResult>(resultadoFiltrado);

            var librosFiltrados =
                Assert.IsType<List<LibroParaResenarDTO>>(okFiltrado.Value);

            var libroEncontrado = Assert.Single(librosFiltrados);
            Assert.Equal(1, libroEncontrado.Id);
            Assert.Equal("Dune", libroEncontrado.Titulo);
        }

        [Fact]
        public async Task GetLibrosParaResenar_badrequest_test()
        {
            var controller = new LibroController(
                _context,
                Mock.Of<ILogger<LibroController>>());

            var generoVacio =
                await controller.GetLibrosParaResenar(" ", null);

            var errorGenero =
                Assert.IsType<BadRequestObjectResult>(generoVacio);

            Assert.IsType<ValidationProblemDetails>(errorGenero.Value);

            var tipoLibroVacio =
                await controller.GetLibrosParaResenar(null, " ");

            var errorTipoLibro =
                Assert.IsType<BadRequestObjectResult>(tipoLibroVacio);

            Assert.IsType<ValidationProblemDetails>(errorTipoLibro.Value);
        }

        private async Task SeedLibrosAsync()
        {
            var cienciaFiccion = new Genero(1, "Ciencia ficción");
            var fantasia = new Genero(2, "Fantasía");

            var minotauro = new Editorial(1, "Minotauro");
            var nova = new Editorial(2, "Nova");

            _context.Generos.AddRange(cienciaFiccion, fantasia);
            _context.Editoriales.AddRange(minotauro, nova);

            _context.Libros.AddRange(
                new Libro(
                    1,
                    "Dune",
                    "Frank Herbert",
                    new DateTime(1965, 8, 1),
                    20m,
                    10m,
                    5,
                    "Tapa blanda",
                    4.5,
                    cienciaFiccion,
                    minotauro),

                new Libro(
                    2,
                    "Fundación",
                    "Isaac Asimov",
                    new DateTime(1951, 1, 1),
                    18m,
                    9m,
                    2,
                    "Tapa dura premium",
                    4.6,
                    cienciaFiccion,
                    nova),

                new Libro(
                    3,
                    "El Hobbit",
                    "J. R. R. Tolkien",
                    new DateTime(1937, 9, 21),
                    15m,
                    7m,
                    0,
                    "Tapa blanda",
                    4.7,
                    fantasia,
                    minotauro)
            );

            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}