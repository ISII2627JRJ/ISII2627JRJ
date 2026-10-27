using System;

namespace AppForSEII.API.DTOs
{
    public class CompraItemDTO
    {
        public CompraItemDTO(int libroId, string titulo, string genero, string editorial, decimal precio, int cantidad)
        {
            LibroId = libroId;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            Precio = precio;
            Cantidad = cantidad;
        }

        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string Genero { get; set; }
        public string Editorial { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CompraItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   Precio == dTO.Precio &&
                   Cantidad == dTO.Cantidad;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Genero, Editorial, Precio, Cantidad);
        }
    }
}