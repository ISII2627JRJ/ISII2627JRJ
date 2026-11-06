using System;
using System.Collections.Generic;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    public class ResenaDetailDTO
    {
        public int Id { get; set; }
        public string TituloResena { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidosCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public IList<ResenaItemDTO> Items { get; set; }
            = new List<ResenaItemDTO>();

        public ResenaDetailDTO(
            int id,
            string tituloResena,
            string nombreCliente,
            string apellidosCliente,
            string telefonoCliente,
            DateTime fecha,
            IList<ResenaItemDTO> items)
        {
            Id = id;
            TituloResena = tituloResena;
            NombreCliente = nombreCliente;
            ApellidosCliente = apellidosCliente;
            TelefonoCliente = telefonoCliente;
            Fecha = fecha;
            Items = items;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResenaDetailDTO other &&
                   Id == other.Id &&
                   TituloResena == other.TituloResena &&
                   NombreCliente == other.NombreCliente &&
                   ApellidosCliente == other.ApellidosCliente &&
                   TelefonoCliente == other.TelefonoCliente &&
                   Fecha == other.Fecha &&
                   Items.SequenceEqual(other.Items);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                TituloResena,
                NombreCliente,
                ApellidosCliente,
                TelefonoCliente,
                Fecha);
        }
    }
}