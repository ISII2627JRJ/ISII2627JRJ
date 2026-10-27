using System;
using System.Collections.Generic;

namespace AppForSEII.API.DTOs
{
    public class ResenaDetailDTO
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public DateTime Fecha { get; set; }
        public IList<ResenaItemDTO> Items { get; set; }

        public ResenaDetailDTO(int id, string nombreCliente, DateTime fecha, IList<ResenaItemDTO> items)
        {
            Id = id;
            NombreCliente = nombreCliente;
            Fecha = fecha;
            Items = items;
        }
    }
}