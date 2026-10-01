using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AppForSEII.API.Models
{
 public class Compra
 {
 [Key]
 public int Id { get; set; }
 [DataType(DataType.Date)]
 public DateTime FechaCompra { get; set; }
 [DataType(DataType.Currency)]
 public decimal PrecioTotal { get; set; }
 [StringLength(10, MinimumLength = 5)] 
 public string? CodigoDescuento { get; set; }
 // Relaciones
 public string ApplicationUserId { get; set; }
 [ForeignKey("ApplicationUserId")]
 public ApplicationUser Cliente { get; set; }
 public int MetodoPagoId { get; set; }
 [ForeignKey("MetodoPagoId")]
 public MetodoPago MetodoPago { get; set; }
 public IList<CompraItem> CompraItems { get; set; }
 }
}
