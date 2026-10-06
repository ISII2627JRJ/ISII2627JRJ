using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DisplayDataType = System.ComponentModel.DataAnnotations.DataType;

namespace AppForSEII.API.Models
{
    public class Compra
    {
        public Compra()
        {
        }
    

        public Compra(int id, DateTime fechaCompra, decimal precioTotal, string? codigoDescuento, string applicationUserId, ApplicationUser usuario, int metodoPagoId, MetodoPago metodoPago, IList<CompraItem> compraItems)
        {
            Id = id;
            FechaCompra = fechaCompra;
            PrecioTotal = precioTotal;
            CodigoDescuento = codigoDescuento;
            ApplicationUserId = applicationUserId;
            Usuario = usuario;
            MetodoPagoId = metodoPagoId;
            MetodoPago = metodoPago;
            CompraItems = compraItems;
        }
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [Key]
        public int Id { get; set; }
        
        [DataType(DisplayDataType.Date)]
        public DateTime FechaCompra { get; set; }
        
        [DataType(DisplayDataType.Currency)]
        public decimal PrecioTotal { get; set; }
        
        [StringLength(10, MinimumLength = 5)] 
        public string? CodigoDescuento { get; set; }
        
        // Relaciones
        public string ApplicationUserId { get; set; }
        
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser Usuario { get; set; }
        
        public int MetodoPagoId { get; set; }
        
        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; }
        
        public IList<CompraItem> CompraItems { get; set; }


        
    }
}
