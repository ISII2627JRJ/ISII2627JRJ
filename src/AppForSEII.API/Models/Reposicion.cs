using System;
using System.Collections.Generic;

namespace AppForSEII.API.Models
{
    public class Reposicion
    {
        public Reposicion()
        {
            ReposicionItems = new List<ReposicionItem>();
        }

        public Reposicion(int id, DateTime fechaReposicion, double precioTotal, string comentario) : this()
        {
            Id = id;
            FechaReposicion = fechaReposicion;
            PrecioTotal = precioTotal;
            Comentario = comentario;
        }

        public int Id { get; set; }
        public DateTime FechaReposicion { get; set; }

        [Range(0,100000)]
        public double PrecioTotal { get; set; }

        [StringLength(100, MinimumLength = 20)]
        public string? Comentario { get; set; }

        // 1. Relación con el usuario
        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser Usuario { get; set; } = null!;

        // 2. Relación con el método de pago
        public int MetodoPagoId { get; set; }
        public virtual MetodoPago MetodoPago { get; set; } = null!;

        // 3. Colección de ítems de la reposición
        public virtual ICollection<ReposicionItem> ReposicionItems { get; set; }
    }
}