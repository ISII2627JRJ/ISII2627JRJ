using System;

namespace AppForSEII.API.Models
{
    public class ReposicionItem
    {
        public ReposicionItem()
        {
        }

        public ReposicionItem(int id, int cantidad, int libroId, int reposicionId)
        {
            Id = id;
            Cantidad = cantidad;
            LibroId = libroId;
            ReposicionId = reposicionId;
        }

        public int Id { get; set; }
        public int Cantidad { get; set; }

        // Relación con Libro
        public int LibroId { get; set; }
        public virtual Libro Libro { get; set; } = null!;

        // Relación con Reposicion
        public int ReposicionId { get; set; }
        public virtual Reposicion Reposicion { get; set; } = null!;
    }
}