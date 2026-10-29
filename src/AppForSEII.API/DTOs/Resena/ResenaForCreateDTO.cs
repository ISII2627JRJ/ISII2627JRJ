using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AppForSEII.API.DTOs
{
    public class ResenaForCreateDTO
    {
        [Required(ErrorMessage = "El identificador del usuario es obligatorio.")]
        public string ApplicationUserId { get; set; }

        [Required]
        public IList<ResenaItemForCreateDTO> Items { get; set; }

        public ResenaForCreateDTO(string applicationUserId, IList<ResenaItemForCreateDTO> items)
        {
            ApplicationUserId = applicationUserId;
            Items = items ?? new List<ResenaItemForCreateDTO>();
        }
    }
}