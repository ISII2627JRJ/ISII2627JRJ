using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DisplayDataType = System.ComponentModel.DataAnnotations.DataType;

namespace AppForSEII.API.Models
{
    public class Libro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Autor { get; set; }

        [DataType(DisplayDataType.Date)]
        public DateTime FechaLanzamiento { get; set; }

        [DataType(DisplayDataType.Currency)]
        public decimal PrecioCompra { get; set; }

        [DataType(DisplayDataType.Currency)]
        public decimal PrecioReposicion { get; set; }

        [Required]
        public int Stock { get; set; }

        [StringLength(50, MinimumLength = 10)]
        public string TipoLibro { get; set; }

        public double CalificacionMedia { get; set; }

        // Relaciones
        public Genero Genero { get; set; }
        public Editorial Editorial { get; set; }
        
        public IList<CompraItem> CompraItems { get; set; }
        public IList<ReponeItem> ReponeItems { get; set; } 
        public IList<ResenaItem> ResenaItems { get; set; }
    }
}