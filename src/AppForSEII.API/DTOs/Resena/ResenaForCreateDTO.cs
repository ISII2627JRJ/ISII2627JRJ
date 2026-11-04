using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class ResenaForCreateDTO
    {
        [Required(ErrorMessage = "El identificador del usuario es obligatorio.")]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título de la reseña es obligatorio.")]
        [StringLength(
            20,
            MinimumLength = 10,
            ErrorMessage = "El título debe tener entre 10 y 20 caracteres.")]
        public string TituloResena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe incluir al menos un libro.")]
        public IList<ResenaItemForCreateDTO> Items { get; set; }
            = new List<ResenaItemForCreateDTO>();
    }
}