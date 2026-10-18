using System;

namespace AppForSEII.API.DTOs
{
    public class LibroParaResenarDTO
    {
        public LibroParaResenarDTO(int id, string titulo, string tipoLibro, string genero, string editorial, string autor, double calificacionMedia, DateTime fechaLanzamiento)
        {
            Id = id;
            Titulo = titulo;
            TipoLibro = tipoLibro;
            Genero = genero;
            Editorial = editorial;
            Autor = autor;
            CalificacionMedia = calificacionMedia;
            FechaLanzamiento = fechaLanzamiento;
        }

        public int Id { get; set; }
        public string Titulo { get; set; }
        public string TipoLibro { get; set; }
        public string Genero { get; set; }
        public string Editorial { get; set; }
        public string Autor { get; set; }
        public double CalificacionMedia { get; set; }
        public DateTime FechaLanzamiento { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is LibroParaResenarDTO dTO &&
                   Id == dTO.Id &&
                   Titulo == dTO.Titulo &&
                   TipoLibro == dTO.TipoLibro &&
                   Genero == dTO.Genero &&
                   Editorial == dTO.Editorial &&
                   Autor == dTO.Autor &&
                   CalificacionMedia == dTO.CalificacionMedia &&
                   FechaLanzamiento == dTO.FechaLanzamiento;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, TipoLibro, Genero, Editorial, Autor, CalificacionMedia, FechaLanzamiento);
        }
    }
}