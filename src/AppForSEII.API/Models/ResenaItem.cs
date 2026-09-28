using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(ResenaId))]
    public class ResenaItem
    {
        public int LibroId { get; set; }

        public int ResenaId { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string? Descripcion { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe ser un valor entre 1 y 5.")]
        public int Calificacion { get; set; }

        public ResenaItem()
        {
        }

        public ResenaItem(int libroId, int resenaId, string? descripcion, int calificacion)
        {
            LibroId = libroId;
            ResenaId = resenaId;
            Descripcion = descripcion;
            Calificacion = calificacion;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResenaItem item &&
                   LibroId == item.LibroId &&
                   ResenaId == item.ResenaId &&
                   Descripcion == item.Descripcion &&
                   Calificacion == item.Calificacion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, ResenaId, Descripcion, Calificacion);
        }
    }
}