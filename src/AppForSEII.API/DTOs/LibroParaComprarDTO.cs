using System;

namespace AppForSEII.API.DTOs
{
    public class LibroParaComprarDTO
    {
        public LibroParaComprarDTO(int id, string titulo, decimal precioCompra, DateTime fechaLanzamiento, string genero, string editorial)
        {
            Id = id;
            Titulo = titulo;
            PrecioCompra = precioCompra;
            FechaLanzamiento = fechaLanzamiento;
            Genero = genero;
            Editorial = editorial;
        }

        public int Id { get; set; }
        public string Titulo { get; set; }
        public decimal PrecioCompra { get; set; }
        public DateTime FechaLanzamiento { get; set; }
        public string Genero { get; set; }
        public string Editorial { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   PrecioCompra == dTO.PrecioCompra &&
                   FechaLanzamiento == dTO.FechaLanzamiento &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, PrecioCompra, FechaLanzamiento, Genero, Editorial);
        }
    }
}