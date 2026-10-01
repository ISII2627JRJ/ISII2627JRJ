using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Editorial
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la editorial es obligatorio")]
        public string Nombre { get; set; }

        public virtual IList<Libro> Libros { get; set; }

        public Editorial()
        {
            Libros = new List<Libro>();
        }

        public Editorial(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
            Libros = new List<Libro>();
        }
    }
}