using System;

namespace AppForSEII.API.DTOs
{
    public class ResenaItemDTO
    {
        public int LibroId { get; set; }
        public string TituloLibro { get; set; }
        public int Puntuacion { get; set; }
        public string TipoLibro { get; set; } = string.Empty;
        public string Editorial { get; set; } = string.Empty;
        public string? Comentario { get; set; }



        public ResenaItemDTO(int libroId, string tituloLibro, string tipoLibro, string editorial, int puntuacion, string? comentario)
        {
            LibroId = libroId;
            TituloLibro = tituloLibro;
            TipoLibro = tipoLibro;
            Editorial = editorial;
            Puntuacion = puntuacion;
            Comentario = comentario;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResenaItemDTO other &&
                   LibroId == other.LibroId &&
                   TituloLibro == other.TituloLibro &&
                   TipoLibro == other.TipoLibro &&
                   Editorial == other.Editorial &&
                   Puntuacion == other.Puntuacion &&
                   Comentario == other.Comentario;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                LibroId,
                TituloLibro,
                TipoLibro,
                Editorial,
                Puntuacion,
                Comentario);
        }
    }
}