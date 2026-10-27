using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    public class ComprasDetailDTO
    {
        public ComprasDetailDTO(int id, string nombreCliente, string apellidosCliente, string direccionEnvio, 
                                string telefono, DateTime fechaCompra, decimal precioTotal, 
                                string metodoPago, string? codigoDescuento, IList<CompraItemDTO> compraItems)
        {
            Id = id;
            NombreCliente = nombreCliente;
            ApellidosCliente = apellidosCliente;
            DireccionEnvio = direccionEnvio;
            Telefono = telefono;
            FechaCompra = fechaCompra;
            PrecioTotal = precioTotal;
            MetodoPago = metodoPago;
            CodigoDescuento = codigoDescuento;
            CompraItems = compraItems;
        }

        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidosCliente { get; set; }
        public string DireccionEnvio { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal PrecioTotal { get; set; }
        public string MetodoPago { get; set; }
        public string? CodigoDescuento { get; set; }
        public IList<CompraItemDTO> CompraItems { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ComprasDetailDTO dTO &&
                   Id == dTO.Id &&
                   NombreCliente == dTO.NombreCliente &&
                   ApellidosCliente == dTO.ApellidosCliente &&
                   DireccionEnvio == dTO.DireccionEnvio &&
                   Telefono == dTO.Telefono &&
                   FechaCompra == dTO.FechaCompra &&
                   PrecioTotal == dTO.PrecioTotal &&
                   MetodoPago == dTO.MetodoPago &&
                   CodigoDescuento == dTO.CodigoDescuento &&
                   CompraItems.SequenceEqual(dTO.CompraItems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, NombreCliente, ApellidosCliente, DireccionEnvio, Telefono, FechaCompra, PrecioTotal, MetodoPago);
        }
    }
}