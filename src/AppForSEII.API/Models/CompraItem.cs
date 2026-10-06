using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(CompraId))]
      public class CompraItem
    {
        public CompraItem()
        {
        }

        public CompraItem(int cantidad, int libroId, Libro libro, int compraId, Compra compra)
        {
            Cantidad = cantidad;
            LibroId = libroId;
            Libro = libro;
            CompraId = compraId;
            Compra = compra;
        }
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 1")] 
        public int Cantidad { get; set; }

        [Required]
        public int LibroId { get; set; }
        [ForeignKey("LibroId")]
        public virtual Libro Libro { get; set; }

        [Required]
        public int CompraId { get; set; }
        [ForeignKey("CompraId")]
        public virtual Compra Compra { get; set; }
    }
}