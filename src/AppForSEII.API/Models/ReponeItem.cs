using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // <-- Es obligatorio importar esto

namespace AppForSEII.API.Models
{
    // 1. Añades la etiqueta justo encima de la clase con las dos propiedades
    [PrimaryKey(nameof(LibroId), nameof(ReposicionId))] 
    public class ReponeItem
    {
        // 2. Defines las dos propiedades que formarán la clave primaria compuesta
        public int LibroId { get; set; }
        
        // (Ajusta este nombre dependiendo de con qué estés relacionando el libro)
        public int ReposicionId { get; set; } 

        // Resto de propiedades normales
        [Required]
        public int CantidadRepuesta { get; set; }
        
        public DateTime FechaReposicion { get; set; }

        public ReponeItem()
        {
        }

        public ReponeItem(int libroId, int reposicionId, int cantidadRepuesta, DateTime fechaReposicion)
        {
            LibroId = libroId;
            ReposicionId = reposicionId;
            CantidadRepuesta = cantidadRepuesta;
            FechaReposicion = fechaReposicion;
        }
    }
}