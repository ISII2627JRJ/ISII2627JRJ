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

                public override bool Equals(object? obj)
        {
            return obj is Genero genero &&
                   Id == genero.Id &&
                   Nombre == genero.Nombre;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Nombre);
        }
    }
}