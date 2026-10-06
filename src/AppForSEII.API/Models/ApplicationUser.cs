using Microsoft.AspNetCore.Identity;

namespace AppForSEII.API.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
    }
  

   public ApplicationUser(string id, string nombre, string apellidos, string direccion, string telefono, string userName)
        {
            Id = id;
            Nombre = nombre;
            Apellidos = apellidos;
            Direccion = direccion;
            Telefono = telefono;
            UserName = userName;
            Email = userName;
        }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(50)]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(100)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone]
        public string Telefono { get; set; }

        // Relación con Compra
        public IList<Compra> Compras { get; set; }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

