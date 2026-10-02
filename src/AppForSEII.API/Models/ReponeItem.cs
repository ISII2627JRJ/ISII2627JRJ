using System;

namespace AppForSEII.API.Models 
{
    public class ReponeItem
    {
        public ReponeItem()
        {
        }

        public ReponeItem(int id, int cantidad, DateTime fechaReposicion)
        {
            Id = id;
            Cantidad = cantidad;
            FechaReposicion = fechaReposicion;
        }

        public int Id { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaReposicion { get; set; }
    }
}