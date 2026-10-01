using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Resena
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual IList<ResenaItem> ResenaItems { get; set; }

        // Constructor vacío
        public Resena()
        {
            ResenaItems = new List<ResenaItem>();
            ApplicationUserId = string.Empty;
        }

        // Constructor con parámetros
        public Resena(int id, DateTime fecha, string applicationUserId)
        {
            Id = id;
            Fecha = fecha;
            ApplicationUserId = applicationUserId;
            ResenaItems = new List<ResenaItem>();
        }
    }
}