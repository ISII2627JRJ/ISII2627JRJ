using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    //un solo DTO para las líneas de la reseña: se usa al crearla y al mostrar su detalle (como RentalItemDTO)
    public class ResenaItemDTO
    {
        public ResenaItemDTO()
        {
        }

        public ResenaItemDTO(int libroId, string titulo, string tipoLibro, string editorial, int calificacion, string? descripcion = null)
        {
            LibroId = libroId;
            Titulo = titulo;
            TipoLibro = tipoLibro;
            Editorial = editorial;
            Calificacion = calificacion;
            Descripcion = descripcion;
        }

        public int LibroId { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string TipoLibro { get; set; } = string.Empty;

        public string Editorial { get; set; } = string.Empty;

        [Required(ErrorMessage = "La calificación es obligatoria.")]
        [Range(1, 5, ErrorMessage = "La calificación debe ser un valor entre 1 y 5.")]
        public int Calificacion { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string? Descripcion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ResenaItemDTO dTO &&
                   LibroId == dTO.LibroId &&
                   Titulo == dTO.Titulo &&
                   TipoLibro == dTO.TipoLibro &&
                   Editorial == dTO.Editorial &&
                   Calificacion == dTO.Calificacion &&
                   Descripcion == dTO.Descripcion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, TipoLibro, Editorial, Calificacion, Descripcion);
        }
    }
}