using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    public class CompraForCreateDTO
    {
        [Required]
        public string CustomerUserName { get; set; } = "default@uclm.es";

        [Required]
        public string Nombre { get; set; } = "Nombre";

        [Required]
        public string Apellidos { get; set; } = "Apellidos";

        [Required]
        public string Direccion { get; set; } = "Direccion";

        [Required]
        public string Telefono { get; set; } = "600000000";

        [Required]
        public int MetodoPago { get; set; }

        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres")]
        public string? CodigoDescuento { get; set; }

        public IList<CompraItemDTO> CompraItems { get; set; } = new List<CompraItemDTO>();

        public double TotalPrice
        {
            get
            {
                return CompraItems.Sum(ci => (double)ci.Precio * ci.Cantidad);
            }
        }

        public CompraForCreateDTO()
        {
            CompraItems = new List<CompraItemDTO>();
        }

        public CompraForCreateDTO(string customerUserName, string nombre, string apellidos, string direccion, string telefono, int metodoPago, string? codigoDescuento, IList<CompraItemDTO> compraItems)
        {
            CustomerUserName = customerUserName ?? throw new ArgumentNullException(nameof(customerUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            MetodoPago = metodoPago;
            CodigoDescuento = codigoDescuento;
            CompraItems = compraItems ?? throw new ArgumentNullException(nameof(compraItems));
        }
    }
}