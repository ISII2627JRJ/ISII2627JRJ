using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class CompraItem
    {
        public CompraItem()
        {
        }

        public CompraItem(int cantidad, string libroId, Libro libro, string compraId, Compra compra)
        {
            Cantidad = cantidad;
            LibroId = libroId;
            Libro = libro;
            CompraId = compraId;
            Compra = compra;
        }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 1")] 
        public int Cantidad { get; set; }

        [Required]
        public string LibroId { get; set; }
        [ForeignKey("LibroId")]
        public virtual Libro Libro { get; set; }

        [Required]
        public string CompraId { get; set; }
        [ForeignKey("CompraId")]
        public virtual Compra Compra { get; set; }
    }
}