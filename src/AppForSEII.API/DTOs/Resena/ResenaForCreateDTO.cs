using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class ResenaForCreateDTO
    {
        public ResenaForCreateDTO()
        {
        }

        public ResenaForCreateDTO(string userName, string nombre, string apellidos, string direccion, string telefono,
            string tituloResena, IList<ResenaItemDTO> resenaItems)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            TituloResena = tituloResena ?? throw new ArgumentNullException(nameof(tituloResena));
            ResenaItems = resenaItems ?? throw new ArgumentNullException(nameof(resenaItems));
        }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título de la reseña es obligatorio.")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "El título debe tener entre 10 y 20 caracteres.")]
        public string TituloResena { get; set; } = string.Empty;

        public IList<ResenaItemDTO> ResenaItems { get; set; } = new List<ResenaItemDTO>();

        public override bool Equals(object? obj)
        {
            return obj is ResenaForCreateDTO dTO &&
                   UserName == dTO.UserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Direccion == dTO.Direccion &&
                   Telefono == dTO.Telefono &&
                   TituloResena == dTO.TituloResena &&
                   ResenaItems.SequenceEqual(dTO.ResenaItems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserName, Nombre, Apellidos, Direccion, Telefono, TituloResena);
        }
    }
}