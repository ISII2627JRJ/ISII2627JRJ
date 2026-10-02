using System;

namespace AppForSEII.API.Models
{
    public class Reposicion
    {
        public Reposicion()
        {
        }

        public Reposicion(int id, DateTime fechaReposicion, double precioTotal, string comentario)
        {
            Id = id;
            FechaReposicion = fechaReposicion;
            PrecioTotal = precioTotal;
            Comentario = comentario;
        }

        public int Id { get; set; }
        public DateTime FechaReposicion { get; set; }
        public double PrecioTotal { get; set; }
        public string Comentario { get; set; }
    }
}