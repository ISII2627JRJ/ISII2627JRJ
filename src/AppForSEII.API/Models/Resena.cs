using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models
{
    public class Resena
    {
        public Resena()
        {
        }

        public Resena(int id, string tituloResena, DateTime fecha, ApplicationUser applicationUser, IList<ResenaItem> resenaItems)
        {
            Id = id;
            TituloResena = tituloResena;
            Fecha = fecha;
            ApplicationUser = applicationUser;
            ApplicationUserId = applicationUser.Id;
            ResenaItems = resenaItems;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string TituloResena { get; set; } = string.Empty;

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public IList<ResenaItem> ResenaItems { get; set; } = new List<ResenaItem>();

        public override bool Equals(object? obj)
        {
            return obj is Resena resena &&
                   Id == resena.Id &&
                   Fecha == resena.Fecha &&
                   TituloResena == resena.TituloResena &&
                   ApplicationUserId == resena.ApplicationUserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Fecha, TituloResena, ApplicationUserId);
        }
    }
}