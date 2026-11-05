using System;
using System.Collections.Generic;

namespace AppForSEII.API.DTOs
{
    public class ReposicionDetailDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaReposicion { get; set; }
        public double PrecioTotal { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? Comentario { get; set; }
        public List<ReposicionItemDTO> ReposicionItems { get; set; } = new List<ReposicionItemDTO>();

        public ReposicionDetailDTO() { }

        public ReposicionDetailDTO(
            int id,
            string nombre,
            string primerApellido,
            string telefono,
            DateTime fechaReposicion,
            double precioTotal,
            string metodoPago,
            string? comentario,
            List<ReposicionItemDTO> reposicionItems)
        {
            Id = id;
            Nombre = nombre;
            PrimerApellido = primerApellido;
            Telefono = telefono;
            FechaReposicion = fechaReposicion;
            PrecioTotal = precioTotal;
            MetodoPago = metodoPago;
            Comentario = comentario;
            ReposicionItems = reposicionItems;
        }
                public override bool Equals(object? obj)
        {
            return obj is ReposicionDetailDTO dTO &&
                   Id == dTO.Id &&
                   Nombre == dTO.Nombre &&
                   PrimerApellido == dTO.PrimerApellido &&
                   Telefono == dTO.Telefono &&
                   FechaReposicion == dTO.FechaReposicion &&
                   PrecioTotal == dTO.PrecioTotal &&
                   MetodoPago == dTO.MetodoPago &&
                   Comentario == dTO.Comentario &&
                   ReposicionItems.SequenceEqual(dTO.ReposicionItems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Nombre, PrimerApellido, Telefono, FechaReposicion, PrecioTotal, MetodoPago, Comentario);
        }
    }
}