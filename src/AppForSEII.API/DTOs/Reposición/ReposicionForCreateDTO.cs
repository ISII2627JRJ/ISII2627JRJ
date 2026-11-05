using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs
{
    public class ReposicionForCreateDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El método de pago es obligatorio.")]
        public string MetodoPago { get; set; } = string.Empty; // Visa, GooglePay o PayPal

        [StringLength(100, MinimumLength = 20, ErrorMessage = "El comentario debe tener entre 20 y 100 caracteres.")]
        public string? Comentario { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un libro a reponer.")]
        public List<ItemDTO> Libros { get; set; } = new List<ItemDTO>();

        // Clase interna anidada para no crear un segundo archivo ni un segundo DTO global
        public class ItemDTO
        {
            [Required]
            public int LibroId { get; set; }

            [Range(2, int.MaxValue, ErrorMessage = "La cantidad a reponer debe ser mayor que 1.")]
            public int Cantidad { get; set; }
        }
    }
}