using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Genero
    {
        public Genero()
        {
        }

        public Genero(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }
    }
}