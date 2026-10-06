using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; 

namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(ReposicionId))] 
    public class ReponeItem
    {
        public int LibroId { get; set; }
        
        public int ReposicionId { get; set; } 

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