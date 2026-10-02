using System;

namespace AppForSEII.API.Models
{
    public class Reposicion
    {
        public int Id { get; set; }
        public DateTime FechaReposicion { get; set; }
        public double PrecioTotal { get; set; }
        public string Comentario { get; set; }
    }
}