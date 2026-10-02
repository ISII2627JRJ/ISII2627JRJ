using System;

namespace AppForSEII.API.Models
{
    public class Visa : MetodoPago
    {
        public string NumeroTarjeta { get; set; }
        public DateTime FechaCaducidad { get; set; }

        public Visa(DateTime fechaCaducidad)
        {
            FechaCaducidad = fechaCaducidad;
        }

        public Visa()
        {
        }
    }
}