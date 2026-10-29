using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class ResenaItemForCreateDTO
    {
        [Required]
        public int LibroId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe ser un valor entre 1 y 5.")]
        public int Calificacion { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string Descripcion { get; set; }

        public ResenaItemForCreateDTO(int libroId, int calificacion, string descripcion)
        {
            LibroId = libroId;
            Calificacion = calificacion;
            Descripcion = descripcion;
        }
    }
}