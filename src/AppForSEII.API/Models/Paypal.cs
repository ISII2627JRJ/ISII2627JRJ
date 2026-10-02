using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class PayPal : MetodoPago
    {
        [Required]
        [Phone]
        public string NumeroTelefono { get; set; }

        public PayPal() 
        { 
        }

        public PayPal(string numeroTelefono)
        {
            NumeroTelefono = numeroTelefono;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PayPal p = (PayPal)obj;
            return NumeroTelefono == p.NumeroTelefono;
        }
    }
}